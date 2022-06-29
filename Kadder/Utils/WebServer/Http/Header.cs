using System.Collections;
using System.Collections.Generic;

namespace Kadder.Utils.WebServer.Http
{
    public class Header : Dictionary<string, string>, IDictionary<string, string>
    {
        public const string ContentLength_Name = "Content-Length";

        private long _contentLength = -1;

        internal long ContentLength
        {
            get
            {
                if (_contentLength < 0)
                {
                    this.TryGetValue(ContentLength_Name, out string strLength);
                    long.TryParse(strLength, out _contentLength);
                }
                return _contentLength;
            }
        }
    }
}
