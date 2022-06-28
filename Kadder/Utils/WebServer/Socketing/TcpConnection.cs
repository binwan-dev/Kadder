using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Text;
using Kadder.WebServer.Http;
using Kadder.WebServer.Http.Pipe;

namespace Kadder.Utils.WebServer.Socketing
{
    public class TcpConnection : IDisposable
    {
        private Socket _socket;
        private SocketAwaitableEventArgs _sendSocketArgs;
        private SocketAwaitableEventArgs _receiveSocketArgs;
        private readonly int _receiveBufferSize;
        private readonly int _sendBufferSize;
        private readonly ILogger _log;
        private int _sending = 0;
        private int _receiving = 0;
        private int _parsing = 0;
        private bool _isDisposed;
        private readonly ConcurrentQueue<ReceiveData> _receiveDataQueue;

        public TcpConnection(Socket socket, ILogger log, int receiveBufferSize, int sendBufferSize)
        {
            _socket = socket;
            _log = log;
            _sendBufferSize = sendBufferSize;
            _receiveBufferSize = receiveBufferSize;

            _sendSocketArgs = new SocketAwaitableEventArgs();
            _sendSocketArgs.AcceptSocket = _socket;
            _receiveSocketArgs = new SocketAwaitableEventArgs();
            _receiveSocketArgs.AcceptSocket = _socket;
            _receiveDataQueue = new ConcurrentQueue<ReceiveData>();
        }

        internal void SetNewFromPool(Socket socket)
        {
            _socket = socket;
            _sendSocketArgs.AcceptSocket = _socket;
            _receiveSocketArgs.AcceptSocket = _socket;
        }

        public async Task DoReceive()
        {
            while (true)
            {
                var buffer = BufferPool.Instance.ArrayPool.Rent(1024 * 1024 * 2);
                var offest = await receiveAsync(buffer);
                if (offest == 0)
                {
                    Dispose();
                    return;
                }
		
                var receiveData = new ReceiveData()
                {
                    Data = buffer,
                    Length = offest
                };
                _receiveDataQueue.Enqueue(receiveData);
		tryParsing();
            }
        }

        private SocketAwaitableEventArgs receiveAsync(Memory<byte> buffer)
        {
            _receiveSocketArgs.SetBuffer(buffer);
            if (!_socket.ReceiveAsync(_receiveSocketArgs))
            {
                _receiveSocketArgs.Complete();
            }
            return _receiveSocketArgs;
        }

  
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
                if (!_receiveDataQueue.TryDequeue(out ReceiveData receiveData)||_isDisposed)
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
                            Dispose();
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
                            Dispose();
                            return;
                        }
                        receiveData.Data = receiveData.Data.Slice(index + 4);
                    }
                    receiveData.Data = request.ParseBody(receiveData.Data);
                    if (request.IsParseBodyDone())
                    {
			BufferPool.Instance.ArrayPool.Return(receiveData.Data.Array);
                        var _ = handleRequest(request);
                        request = null;
                    }
                }
                break;
            }
        }

        private async Task handleRequest(Request request)
        {
	    var response = new Response(_socket);
	    var context = new HttpContext(request, response);
	    await new InitPipe().HandlerAsync(context);
	    Dispose();
	}

        private void CompletedSend(object sender, SocketAsyncEventArgs e)
        {
            throw new NotImplementedException();
        }
	
        public void Dispose()
        {
	    if(_isDisposed)
                return;

            _sendSocketArgs.AcceptSocket = null;
            _receiveSocketArgs.AcceptSocket = null;
            _receiveDataQueue.Clear();
            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();
            _socket.Dispose();
            _socket = null;
            TcpConnectionPool.Instance.ReturnConnection(this);
        }

        private struct ReceiveData
        {
            public ArraySegment<byte> Data { get; set; }

            public int Length { get; set; }
        }
    }
}
