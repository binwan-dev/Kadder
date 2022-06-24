using System;
using System.IO;
using System.Text;

namespace Kadder.WebServer.Http
{
    public class Request
    {


        public string Version { get; set; }

        public string Method { get; set; }

        public string Path { get; set; }

        public string Url { get; set; }

        public Header Header { get; set; }

        public Stream Body { get; set; }

        public void ParseURI(ArraySegment<byte> protocol)
        {
            if (protocol.Count == 0)
                return;
            var no = 0;
            var index = 0;
            for (var i = 0; i < protocol.Count; i++)
            {
                if (protocol[i] != ' ' && i < protocol.Count - 1)
                    continue;

                if (no == 0)
                    Method = Encoding.UTF8.GetString(protocol.Slice(index, (i - index))).ToUpper();
                if (no == 1)
                    Url = Encoding.UTF8.GetString(protocol.Slice(index, (i - index)));
                if (no == 2)
                    Version = Encoding.UTF8.GetString(protocol.Slice(index, (i - index)));
                no += 1;
                index = i + 1;
            }
        }

        public void ParseHeader(ArraySegment<byte> data)
        {
            Header = new Header();

            if (data.Count == 0)
                return;

            var key_i = 0;
            var val_i = 0;
            for (var i = 0; i < data.Count; i++)
            {
                if (data[i] == ':' && val_i == key_i)
                    val_i = i + 1;
                if (i == 0 || data[i - 1] != '\r' || data[i] != '\n')
                    continue;

                var key = Encoding.UTF8.GetString(data.Slice(key_i, val_i - key_i - 1));
                if (data[val_i] == ' ')
                    val_i += 1;
                var val = Encoding.UTF8.GetString(data.Slice(val_i, i - val_i - 1));
                Header.Add(key, val);

                key_i = i + 1;
                val_i = key_i;
            }
        }

        public ArraySegment<byte> ParseBody(ArraySegment<byte> data)
        {
            if (Header.ContentLength == 0)
                return data;
            if (Body == null)
                Body = new MemoryStream();

            var needLength = Header.ContentLength - Body.Length;
            if (data.Count > needLength)
            {
                Body.Write(data.Slice(0, (int)needLength));
                return data.Slice((int)needLength + 1);
            }
            else
            {
                Body.Write(data);
                return data.Slice(data.Count - 1);
            }
        }

        public bool IsParseBodyDone()
        {
            if (Header.ContentLength == 0)
                return true;
            if (Header.ContentLength == Body.Length)
                return true;
            return false;
        }

        public bool IsValid()
        {
            if (string.IsNullOrWhiteSpace(Version) || string.IsNullOrWhiteSpace(Method) || string.IsNullOrWhiteSpace(Url))
                return false;
            if (Version != "HTTP/1.0" && Version != "HTTP/1.1" && Version != "HTTP/2.0")
                return false;
            if (Method != "GET" && Method != "POST" && Method != "HEAD" && Method != "PUT" && Method != "DELETE" && Method != "OPTIONS")
                return false;
            return true;
        }

        public bool IsValidHeader()
        {
            if (Header != null)
                return true;
            return false;
        }
    }
}