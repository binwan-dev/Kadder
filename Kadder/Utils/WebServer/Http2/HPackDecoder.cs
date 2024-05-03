using System;
using System.Collections.Generic;
using System.Text;
using Kadder.Utils.WebServer.Http2.HPack;

namespace Kadder.Utils.WebServer.Http2
{
    public class HPackDecoder
    {
        private readonly IDictionary<int, HeaderItem> _staticTable;
        private readonly IDictionary<int, HeaderItem> _dynamicTable;

        public HPackDecoder()
        {
            _staticTable = initStaticTable();
            _dynamicTable = new Dictionary<int, HeaderItem>();
        }

        public IDictionary<string, string> Decode(ArraySegment<byte> buffer)
        {
            var header = new Dictionary<string, string>();
            while (buffer.Offset < buffer.Count)
            {
                var i = 0;
                var item = buffer[i];
                var seven = item.GetBinary(7);
                var six = item.GetBinary(6);
                var five = item.GetBinary(5);
                var four = item.GetBinary(4);

                var index = 0;
                if (!seven && six)
                {
                    index = item ^ 64;


                    if (index != 0)
                    {
                        var ishuffman = false;
                        var length = buffer[i + 1];
                        if (buffer[i + 1] >= 128)
                        {
                            length = (byte) (length ^ 128);
                            ishuffman = true;
                        }

                        var data = Encoding.UTF8.GetString(buffer.Slice(i + 2, length));
                        buffer = buffer.Slice(i + 2 + length);
                    }
                }


                if (!seven && !six && !five && four)
                {
                    index = item ^ 16;
                    var ishuffman = false;
                    var length = buffer[i + 1];
                    if (buffer[i + 1] >= 128)
                    {
                        length = (byte) (length ^ 128);
                        ishuffman = true;
                    }

                    var data = buffer.Slice(i + 2, length);
                    data = Huffman.Decoder.Decode(data.ToArray());
                    buffer = buffer.Slice(i + 2 + length);
                }
            }

            return new Dictionary<string, string>();
        }

        private IDictionary<int, HeaderItem> initStaticTable()
        {
            var staticTable = new Dictionary<int, HeaderItem>();
            staticTable.Add(1, new HeaderItem() {Name = ":authority", Value = string.Empty});
            staticTable.Add(2, new HeaderItem() {Name = ":method", Value = "GET"});
            staticTable.Add(3, new HeaderItem() {Name = ":method", Value = "POST"});
            staticTable.Add(4, new HeaderItem() {Name = ":path", Value = "/"});
            staticTable.Add(5, new HeaderItem() {Name = ":path", Value = "/index.html"});
            staticTable.Add(6, new HeaderItem() {Name = ":scheme", Value = "http"});
            staticTable.Add(7, new HeaderItem() {Name = ":scheme", Value = "https"});
            staticTable.Add(8, new HeaderItem() {Name = ":status", Value = "200"});
            staticTable.Add(9, new HeaderItem() {Name = ":status", Value = "204"});
            staticTable.Add(10, new HeaderItem() {Name = ":status", Value = "206"});
            staticTable.Add(11, new HeaderItem() {Name = ":status", Value = "304"});
            staticTable.Add(12, new HeaderItem() {Name = ":status", Value = "400"});
            staticTable.Add(13, new HeaderItem() {Name = ":status", Value = "404"});
            staticTable.Add(14, new HeaderItem() {Name = ":status", Value = "500"});
            staticTable.Add(15, new HeaderItem() {Name = ":accept-charset", Value = string.Empty});
            staticTable.Add(16, new HeaderItem() {Name = ":accept-encoding", Value = "gzip, deflate"});
            staticTable.Add(17, new HeaderItem() {Name = ":accept-language", Value = string.Empty});
            staticTable.Add(18, new HeaderItem() {Name = ":accept-ranges", Value = string.Empty});
            staticTable.Add(19, new HeaderItem() {Name = ":accept", Value = string.Empty});
            staticTable.Add(20, new HeaderItem() {Name = ":access-control-allow-origin", Value = string.Empty});
            staticTable.Add(21, new HeaderItem() {Name = ":age", Value = string.Empty});
            staticTable.Add(22, new HeaderItem() {Name = ":allow", Value = string.Empty});
            staticTable.Add(23, new HeaderItem() {Name = ":authorization", Value = string.Empty});
            staticTable.Add(24, new HeaderItem() {Name = ":cache-control", Value = string.Empty});
            staticTable.Add(25, new HeaderItem() {Name = ":content-disposition", Value = string.Empty});
            staticTable.Add(26, new HeaderItem() {Name = ":content-encoding", Value = string.Empty});
            staticTable.Add(27, new HeaderItem() {Name = ":content-language", Value = string.Empty});
            staticTable.Add(28, new HeaderItem() {Name = ":content-length", Value = string.Empty});
            staticTable.Add(29, new HeaderItem() {Name = ":content-location", Value = string.Empty});
            staticTable.Add(30, new HeaderItem() {Name = ":content-range", Value = string.Empty});
            staticTable.Add(31, new HeaderItem() {Name = ":content-type", Value = string.Empty});
            staticTable.Add(32, new HeaderItem() {Name = ":cookie", Value = string.Empty});
            staticTable.Add(33, new HeaderItem() {Name = ":date", Value = string.Empty});
            staticTable.Add(34, new HeaderItem() {Name = ":etag", Value = string.Empty});
            staticTable.Add(35, new HeaderItem() {Name = ":expect", Value = string.Empty});
            staticTable.Add(36, new HeaderItem() {Name = ":expires", Value = string.Empty});
            staticTable.Add(37, new HeaderItem() {Name = ":from", Value = string.Empty});
            staticTable.Add(38, new HeaderItem() {Name = ":host", Value = string.Empty});
            staticTable.Add(39, new HeaderItem() {Name = ":if-match", Value = string.Empty});
            staticTable.Add(40, new HeaderItem() {Name = ":if-modified-since", Value = string.Empty});
            staticTable.Add(41, new HeaderItem() {Name = ":if-none-match", Value = string.Empty});
            staticTable.Add(42, new HeaderItem() {Name = ":if-range", Value = string.Empty});
            staticTable.Add(43, new HeaderItem() {Name = ":if-unmodified-since", Value = string.Empty});
            staticTable.Add(44, new HeaderItem() {Name = ":last-modified", Value = string.Empty});
            staticTable.Add(45, new HeaderItem() {Name = ":link", Value = string.Empty});
            staticTable.Add(46, new HeaderItem() {Name = ":location", Value = string.Empty});
            staticTable.Add(47, new HeaderItem() {Name = ":max-forwards", Value = string.Empty});
            staticTable.Add(48, new HeaderItem() {Name = ":proxy-authenticate", Value = string.Empty});
            staticTable.Add(49, new HeaderItem() {Name = ":proxy-authorization", Value = string.Empty});
            staticTable.Add(50, new HeaderItem() {Name = ":range", Value = string.Empty});
            staticTable.Add(51, new HeaderItem() {Name = ":referer", Value = string.Empty});
            staticTable.Add(52, new HeaderItem() {Name = ":refresh", Value = string.Empty});
            staticTable.Add(53, new HeaderItem() {Name = ":retry-after", Value = string.Empty});
            staticTable.Add(54, new HeaderItem() {Name = ":server", Value = string.Empty});
            staticTable.Add(55, new HeaderItem() {Name = ":set-cookie", Value = string.Empty});
            staticTable.Add(56, new HeaderItem() {Name = ":strict-transport-security", Value = string.Empty});
            staticTable.Add(57, new HeaderItem() {Name = ":transfer-encoding", Value = string.Empty});
            staticTable.Add(58, new HeaderItem() {Name = ":user-agent", Value = string.Empty});
            staticTable.Add(59, new HeaderItem() {Name = ":vary", Value = string.Empty});
            staticTable.Add(60, new HeaderItem() {Name = ":via", Value = string.Empty});
            staticTable.Add(61, new HeaderItem() {Name = ":www-authenticate", Value = string.Empty});
            return staticTable;
        }

        public struct HeaderItem
        {
            public string Name { get; set; }

            public string Value { get; set; }
        }
    }
}