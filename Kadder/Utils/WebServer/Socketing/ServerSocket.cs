using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Kadder.WebServer.Socketing
{
    public class ServerSocket
    {
        private readonly Socket _socket;
        private readonly IPEndPoint _listenEndPoint;
        private readonly SocketAsyncEventArgs _acceptingSocketArgs;
        private int _starting = 0;
        private int _started = 0;
        private readonly int _listenPendingConns;
        private readonly ILogger<ServerSocket> _log;
        private readonly ILogger<TcpConnection> _connectionLog;

        public ServerSocket(string listenAddress, int port, ILogger<ServerSocket> log, ILogger<TcpConnection> connectionLog, int listenPendingConns = 100)
        {
            _listenPendingConns = listenPendingConns;
            _log = log;
            _connectionLog = connectionLog;
            _listenEndPoint = new IPEndPoint(IPAddress.Parse(listenAddress), port);
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _acceptingSocketArgs = new SocketAsyncEventArgs();
            _acceptingSocketArgs.Completed += CompletedAccept;
        }

        public void Start()
        {
            if (_started != 0)
                return;
            if (Interlocked.CompareExchange(ref _starting, 1, 0) != 0)
                return;

            try
            {
                tryStarting();
                Interlocked.Exchange(ref _started, 1);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"The tcp socket start listen failed! listen endpoint: {_listenEndPoint.ToString()}", ex);
            }
            finally
            {
                Interlocked.Exchange(ref _starting, 0);
            }
        }

        private void tryStarting()
        {
            _socket.Bind(_listenEndPoint);
            _socket.Listen(_listenPendingConns);
            Task.Factory.StartNew(startAccepting);
            _log.LogInformation($"socket listening for {_listenEndPoint.ToString()}");
            _log.LogInformation("waiting socket accept....");
        }

        private async Task startAccepting()
        {
            var args = new SocketAwaitableEventArgs();
            while (true)
            {
                await acceptAsync(args);
                ProcessAccept(args.AcceptSocket, args.SocketError);
                args.AcceptSocket = null;
            }

            // try
            // {
            //     if (!_socket.AcceptAsync(_acceptingSocketArgs))
            //         CompletedAccept(_socket, _acceptingSocketArgs);
            // }
            // catch (Exception ex)
            // {
            //     _log.LogError(ex, "accept failed! will be reaccept for 2 seconds!");
            //     System.Threading.Thread.Sleep(2000);
            //     startAccepting();
            // }
        }

        private SocketAwaitableEventArgs acceptAsync(SocketAwaitableEventArgs args)
        {
            if(!_socket.AcceptAsync(args))
                args.Complete();
            return args;
        }


        private void CompletedAccept(object sender, SocketAsyncEventArgs e)
        {
            try
            {
                ProcessAccept(e.AcceptSocket, e.SocketError);
                e.AcceptSocket = null;
            }
            catch (Exception ex)
            {
                SocketHelper.ShutdownSocket(e.AcceptSocket, e.SocketError, $"the socket({e.AcceptSocket.RemoteEndPoint.ToString()}) accept has an know error!", _log, ex);
            }
            finally
            {
                startAccepting();
            }
        }

        private void ProcessAccept(Socket socket, SocketError socketError)
        {
            if (socketError != SocketError.Success)
            {
                SocketHelper.ShutdownSocket(socket, socketError, $"the socket({socket.RemoteEndPoint.ToString()}) accept has a socket error! SocketError: {socketError.ToString()}", _log);
                return;
            }

            var connection = new TcpConnection(socket, _connectionLog, 1024 * 1024 * 2, 1024 * 1024 * 2);
            _ = handleConnection(connection);
        }

        private async Task handleConnection(TcpConnection connection)
        {
            await connection.DoReceive();
        }
    }
}
