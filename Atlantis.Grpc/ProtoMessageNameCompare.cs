using System.Collections.Generic;

namespace Atlantis.Grpc
{
    public class ProtoMessageNameCompare : IComparer<string>
    {
        public int Compare(string x, string y)
        {
           return string.CompareOrdinal(x,y);
        }
    }
}
