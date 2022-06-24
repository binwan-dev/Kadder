using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using Kadder.WebServer.Http;
using Kadder.WebServer.Http.Pipe;

namespace Kadder.WebServer.Socketing
{
    public class TcpConnection 
    {
        private readonly Socket _socket;
        private readonly SocketAsyncEventArgs _sendSocketArgs;
        private readonly SocketAsyncEventArgs _receiveSocketArgs;
        private readonly int _receiveBufferSize;
        private readonly int _sendBufferSize;
        private readonly ILogger _log;
        private int _sending = 0;
        private int _receiving = 0;
        private int _parsing = 0;
        private readonly ConcurrentQueue<ReceiveData> _receiveDataQueue;
        private readonly SocketAwaitableEventArgs _receiveArgs;

        public TcpConnection(Socket socket, ILogger log, int receiveBufferSize, int sendBufferSize)
        {
            _socket = socket;
            _log = log;
            _sendBufferSize = sendBufferSize;
            _receiveBufferSize = receiveBufferSize;

            _sendSocketArgs = new SocketAsyncEventArgs();
            _sendSocketArgs.AcceptSocket = _socket;
            _sendSocketArgs.Completed += CompletedSend;
            _receiveSocketArgs = new SocketAsyncEventArgs();
            _receiveSocketArgs.AcceptSocket = _socket;
            _receiveSocketArgs.Completed += CompletedReceive;
            _receiveDataQueue = new ConcurrentQueue<ReceiveData>();
            _receiveArgs = new SocketAwaitableEventArgs();
        }

        public async Task DoReceive()
        {
            var args = new SocketAwaitableEventArgs();
            while (true)
            {
                var buffer = new byte[1024 * 1024 * 2];
                var offest=await ReceiveAsync(buffer);
                if (offest == 0)
                {
                    SocketHelper.ShutdownSocket(_socket, SocketError.AccessDenied, "Socket fin close", _log);
                    return;
                }
		
		var receiveData = new ReceiveData()
		{
		    Data = buffer,
		    Length = offest
		};
		_receiveDataQueue.Enqueue(receiveData);
		_receiveArgs.SetBuffer(null);
		tryParsing();
            }
        }

        private SocketAwaitableEventArgs ReceiveAsync(Memory<byte> buffer)
        {
            _receiveArgs.SetBuffer(buffer);
            if (!_socket.ReceiveAsync(_receiveArgs))
            {
                _receiveArgs.Complete();
            }
            return _receiveArgs;
        }

        private async Task tryReceive()
        {
            if (Interlocked.CompareExchange(ref _receiving, 1, 0) != 0)
                return;

            try
            {
                _receiveSocketArgs.SetBuffer(new byte[_receiveBufferSize], 0, _receiveBufferSize);
                if (!_socket.ReceiveAsync(_receiveSocketArgs))
                    CompletedReceive(_socket, _receiveSocketArgs);
            }
            catch (Exception ex)
            {
                SocketHelper.ShutdownSocket(_socket, SocketError.Success, $"the socket({_socket.RemoteEndPoint.ToString()}) receive has an know error!", _log, ex);
            }
        }

        private void CompletedReceive(object sender, SocketAsyncEventArgs e) => ProcessReceive(e);

        private void ProcessReceive(SocketAsyncEventArgs receiveArgs)
        {
            if (receiveArgs.SocketError != SocketError.Success)
            {
                SocketHelper.ShutdownSocket(receiveArgs.AcceptSocket, receiveArgs.SocketError, $"the socket({receiveArgs.AcceptSocket.RemoteEndPoint.ToString()}) receive msg failed!", _log);
                return;
            }
            if (receiveArgs.BytesTransferred == 0)
            {
                SocketHelper.ShutdownSocket(receiveArgs.AcceptSocket, SocketError.Disconnecting, $"the socket({receiveArgs.AcceptSocket.RemoteEndPoint.ToString()}) disconnected!", _log);
                return;
            }

            var receiveData = new ReceiveData()
            {
                Data = receiveArgs.Buffer,
                Length = receiveArgs.BytesTransferred
            };
            _receiveDataQueue.Enqueue(receiveData);
            receiveArgs.SetBuffer(null);
            tryParsing();

            exitReceive();
            tryReceive();
        }

        private void exitReceive() => Interlocked.Exchange(ref _receiving, 0);

        private void tryParsing()
        {
            if (Interlocked.CompareExchange(ref _parsing, 1, 0) != 0)
                return;

            try
            {
                parsing();
            }
            catch (Exception ex)
            {
                _log.LogError(ex, $"socket({_socket.RemoteEndPoint.ToString()}) parsing data has an unknow error!");
            }
            finally
            {
                Interlocked.Exchange(ref _parsing, 0);
            }
        }

        private void parsing()
        {
            Request request = null;
            while (true)
            {
                if (!_receiveDataQueue.TryDequeue(out ReceiveData receiveData))
                    return;

                while (true)
                {
                    if (receiveData.Data.Offset >= receiveData.Length)
                        break;

                    if (request == null)
                    {
                        request = new Request();
                        var index = 0;
                        for (; index < receiveData.Length; index++)
                        {
                            if (index > 0 && receiveData.Data[index - 1] == '\r' && receiveData.Data[index] == '\n')
                            {
                                request.ParseURI(receiveData.Data.Slice(0, index));
                                break;
                            }
                        }
                        if (!request.IsValid())
                        {
                            SocketHelper.ShutdownSocket(_socket, SocketError.ConnectionReset, "Invalid http request", _log);
                            return;
                        }
                        receiveData.Data = receiveData.Data.Slice(index + 1);
                        char s = (char)receiveData.Data[0];

                        for (; index < receiveData.Length; index++)
                        {
                            if (receiveData.Data.Count > index + 3 &&
                                receiveData.Data[index] == '\r' && receiveData.Data[index + 1] == '\n' &&
                                receiveData.Data[index + 2] == '\r' && receiveData.Data[index + 3] == '\n')
                            {
                                request.ParseHeader(receiveData.Data.Slice(0, index + 2));
                                break;
                            }
                        }
                        if (!request.IsValidHeader())
                        {
                            SocketHelper.ShutdownSocket(_socket, SocketError.ConnectionAborted, "Invalid http request!", _log);
                            return;
                        }
                        receiveData.Data = receiveData.Data.Slice(index + 4);
                    }
                    receiveData.Data = request.ParseBody(receiveData.Data);
                    if (request.IsParseBodyDone())
                    {
                        var receiveRequest = request;
                        Task.Factory.StartNew(() =>
                        {
                            var response = new Response(_socket);
                            var context = new HttpContext(receiveRequest, response);
                            new InitPipe().Handler(context);
                        });
                        request = null;
                    }
                }
                break;
            }
        }

        private void CompletedSend(object sender, SocketAsyncEventArgs e)
        {
            throw new NotImplementedException();
        }

        private struct ReceiveData
        {
            public ArraySegment<byte> Data { get; set; }

            public int Length { get; set; }
        }
    }
}
