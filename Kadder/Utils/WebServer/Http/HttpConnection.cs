using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using Kadder.Utils.WebServer.Socketing;
using System.Text;

namespace Kadder.Utils.WebServer.Http
{
    public class HttpConnection:TcpConnection,IDisposable
    {
        private readonly HttpConnectionOptions _options;
        private Request _request;

        public HttpConnection(Socket socket, HttpConnectionOptions options):base(socket)
        {
            _options = options;
        }

        public override async Task DoReceiveAsync()
        {
            while (true)
            {
                var buffer = BufferPool.Instance.ArrayPool.Rent(1024 * 1024 * 2);
                var offest = await receiveAsync(buffer);
                if (offest == 0)
                    return;

                var result = parseRequest(new ArraySegment<byte>(buffer, 0, offest));
                if (!result.Status||!_request.IsURIParsed)
                    break;
                if (!_request.IsBodyParsed)
                {
                    BufferPool.Instance.ArrayPool.Return(buffer);
                    continue;
                }

                await handler(_request);
                _request = null;
                break;
            }

            var _ = Task.Run(Dispose);
        }

        private async Task handler(Request request)
        {
            var response = new Response(_socket);
            response.Version = request.Version;
            response.StatusCode = 200;
            response.Description = "OK";
            await response.WriteAsync(Encoding.UTF8.GetBytes("hello world!"));
            await response.FlushAsync();
        }

        private (bool Status,ArraySegment<byte> Buffer) parseRequest(ArraySegment<byte> buffer)
        {
	    if(_request==null)
                _request = new Request();

	    var index = 0;
            if (!_request.IsURIParsed)
            {
                for (; index < buffer.Count; index++)
                {
                    if (index > 0 && buffer[index - 1] == '\r' && buffer[index] == '\n')
                    {
                        _request.ParseURI(buffer.Slice(0, index));
                        break;
                    }
                }

                if (!_request.IsValid())
                    return (false, buffer);
            }
	    buffer = buffer.Slice(index + 1);

            if (!_request.IsHeaderParsed && _request.IsURIParsed)
            {
                for (; index < buffer.Count; index++)
                {
                    if (buffer.Count > index + 3 &&
                    buffer[index] == '\r' && buffer[index + 1] == '\n' &&
                    buffer[index + 2] == '\r' && buffer[index + 3] == '\n')
                    {
                        _request.ParseHeader(buffer.Slice(0, index + 2));
                        _request.IsHeaderParsed = true;
                        break;
                    }
                }
                if (!_request.IsValidHeader())
                    return (false, buffer);
            }
            buffer = buffer.Slice(index + 4);

            if (!_request.IsBodyParsed&&_request.IsURIParsed&&_request.IsHeaderParsed)
            {
                buffer = _request.ParseBody(buffer);
		if(_request.Header.ContentLength==_request.Body.Length)
                    _request.IsBodyParsed = true;
            }
            return (true, buffer);
        }

	public void Dispose()
        {
            _receiveSocketArgs.AcceptSocket = null;
            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();
            _socket.Dispose();
            _socket = null;
            HttpConnectionPool.Instance.ReturnConnection(this);
        }

    }
}
