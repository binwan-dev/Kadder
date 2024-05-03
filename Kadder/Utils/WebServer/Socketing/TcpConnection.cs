using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Kadder.Utils.WebServer.Socketing
{
    public abstract class TcpConnection 
    {
        protected Socket _socket;
        private SocketAwaitableEventArgs _sendSocketArgs;
        protected SocketAwaitableEventArgs _receiveSocketArgs;
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

        public TcpConnection(Socket socket)
        {
            _socket = socket;
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

        public abstract Task DoReceiveAsync();

        protected SocketAwaitableEventArgs receiveAsync(Memory<byte> buffer)
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

        private struct ReceiveData
        {
            public ArraySegment<byte> Data { get; set; }

            public int Length { get; set; }
        }
    }
}
