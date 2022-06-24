using System;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace Kadder.WebServer.Http
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

        public void Write(byte[] body)
        {
            Body.Value.Write(body);
        }

        public void Flush()
        {
            Header.Add("Content-Length", Body.Value.Length.ToString());
            var stream = genHttpStream();
            _socket.Send(stream.GetBuffer(), 0, (int)stream.Length, SocketFlags.None);
        }

        private MemoryStream genHttpStream()
        {
            var stream = new MemoryStream();
            stream.Write(Encoding.UTF8.GetBytes($"{Version} {StatusCode} OK\r\n"));
            foreach (var item in Header)
            {
                stream.Write(Encoding.UTF8.GetBytes($"{item.Key}: {item.Value}\r\n"));
            }
            stream.Write(new byte[2] { 13, 10 });
            stream.Write(Body.Value.GetBuffer(), 0, (int)Body.Value.Length);
            return stream;
        }
    }
}