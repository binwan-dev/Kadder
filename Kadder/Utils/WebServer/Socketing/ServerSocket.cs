using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Kadder.Utils.WebServer.Socketing
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

        public ServerSocket(string listenAddress, int port, ILogger<ServerSocket> log, ILogger<TcpConnection> connectionLog, int listenPendingConns = -1)
        {
            _listenPendingConns = listenPendingConns;
            _log = log;
            _connectionLog = connectionLog;
            _listenEndPoint = new IPEndPoint(IPAddress.Parse(listenAddress), port);
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _acceptingSocketArgs = new SocketAsyncEventArgs();
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
	    if(_listenPendingConns>0)
		_socket.Listen(_listenPendingConns);
	    else
                _socket.Listen();
            var _ = startAccepting;
            _log.LogInformation($"socket listening for {_listenEndPoint.ToString()}");
            _log.LogInformation("waiting socket accept....");
        }

        private async Task startAccepting()
        {
            var args = new SocketAwaitableEventArgs();
            while (true)
            {
                await acceptAsync(args);
                var _ = processAccept(args.AcceptSocket, args.SocketError);
            }
        }

        private SocketAwaitableEventArgs acceptAsync(SocketAwaitableEventArgs args)
        {
            if(!_socket.AcceptAsync(args))
                args.Complete();
            return args;
        }

        private async Task processAccept(Socket socket, SocketError socketError)
        {
            if (socketError != SocketError.Success)
            {
                SocketHelper.ShutdownSocket(socket, socketError, $"the socket({socket.RemoteEndPoint.ToString()}) accept has a socket error! SocketError: {socketError.ToString()}", _log);
                return;
            }

            var connection = TcpConnectionPool.Instance.GetOrCreateConnection(socket, _connectionLog, 1024 * 1024 * 2, 1024 * 1024 * 2);
            await connection.DoReceive();
        }
    }
}
