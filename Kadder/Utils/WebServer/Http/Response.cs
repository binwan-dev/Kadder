using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Kadder.Utils.WebServer.Http
{
    public class Response
    {
        private readonly Socket _socket;

        public Response(Socket socket)
        {
            _socket = socket;
            Header = new Header();
            Body = new Lazy<MemoryStream>(() => new MemoryStream());
        }

        public string Version { get; set; }

        public int StatusCode { get; set; }

        public string Description { get; set; }

        public Header Header { get; set; }

        public Lazy<MemoryStream> Body { get; set; }

        public ValueTask WriteAsync(byte[] body)
        {
            return Body.Value.WriteAsync(body);
        }

        public async Task FlushAsync()
        {
            Header.Add("Content-Length", Body.Value.Length.ToString());
            using var stream = await genHttpStreamAsync();
            await _socket.SendAsync(stream.GetBuffer(), SocketFlags.None);
        }

        private async Task<MemoryStream> genHttpStreamAsync()
        {
            var stream = new MemoryStream();
            await stream.WriteAsync(Encoding.UTF8.GetBytes($"{Version} {StatusCode} OK\r\n"));
            foreach (var item in Header)
            {
                await stream.WriteAsync(Encoding.UTF8.GetBytes($"{item.Key}: {item.Value}\r\n"));
            }
            await stream.WriteAsync(new byte[2] { 13, 10 });
            await stream.WriteAsync(Body.Value.GetBuffer(), 0, (int)Body.Value.Length);
            return stream;
        }
    }
}
