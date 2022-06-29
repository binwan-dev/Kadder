using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Text;
using Kadder.WebServer.Http;
using Kadder.WebServer.Http.Pipe;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Buffers;

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
        private readonly Queue<ReceiveData> _receiveDataQueue;
	public Stopwatch stopWatch = new Stopwatch();
        public static byte[] sendData;

        static TcpConnection()
        {
	    var bodyData = Encoding.UTF8.GetBytes("Hello World");
            var s = new MemoryStream();
            s.Write(Encoding.UTF8.GetBytes($"HTTP/1.1 200 OK\r\n"));
            s.Write(Encoding.UTF8.GetBytes($"Content-Length: {bodyData.Length}\r\n"));
            s.Write(new byte[2] { 13, 10 });
	    s.Write(bodyData);
            sendData = s.ToArray();
	}

        public TcpConnection(Socket socket, ILogger log)
        {
            _socket = socket;
            _log = log;
            // _sendBufferSize = sendBufferSize;
            // _receiveBufferSize = receiveBufferSize;

            _sendSocketArgs = new SocketAwaitableEventArgs();
            _sendSocketArgs.AcceptSocket = _socket;
            _receiveSocketArgs = new SocketAwaitableEventArgs();
            _receiveSocketArgs.AcceptSocket = _socket;
            _receiveDataQueue = new Queue<ReceiveData>();
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
                    Console.WriteLine(100);
                    return;
                } 

                var _= handleRequest(buffer);


                // var receiveData = new ReceiveData()
                // {XSXS
                //     Data = buffer,
                //     Length = offest
                // };
                // _receiveDataQueue.Enqueue(receiveData);
                // var _ = Task.Run(tryParsing);
            }
        }

        public SocketAwaitableEventArgs receiveAsync(Memory<byte> buffer)
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
			// stopWatch.Restart();
                        request = new Request();
			// stopWatch.Stop();
			// if(stopWatch.ElapsedMilliseconds>0)
			    // Console.WriteLine($"handle used {stopWatch.ElapsedMilliseconds} ms");
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
                        var _ = handleRequest(receiveData.Data.Array);
                        request = null;
                    }
                }
                break;
            }
        }

        private async Task handleRequest(byte[] buffer)
        {
            // var s = new Stopwatch();
            // s.Start();
            // var response = new Response(_socket);
            // s.Stop();
            // if(s.ElapsedMilliseconds>0)
            //     Console.WriteLine(s.ElapsedMilliseconds);
            // var context = new HttpContext(request, response);
            // await new InitPipe().HandlerAsync(context);

            // var bodyData = Encoding.UTF8.GetBytes("Hello World");
            // await _socket.SendAsync(Encoding.UTF8.GetBytes($"HTTP/1.1 200 OK\r\n"),SocketFlags.None);
            // await _socket.SendAsync(Encoding.UTF8.GetBytes($"Content-Length: {bodyData.Length}\r\n"), SocketFlags.None);
            // await _socket.SendAsync(new byte[2] { 13, 10 },SocketFlags.None);
            // await _socket.SendAsync(bodyData,SocketFlags.None);

            BufferPool.Instance.ArrayPool.Return(buffer);
            await _socket.SendAsync(sendData, SocketFlags.None);
            _socket.Shutdown(SocketShutdown.Both);
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
