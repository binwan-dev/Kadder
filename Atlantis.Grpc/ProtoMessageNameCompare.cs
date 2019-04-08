using System.Collections.Generic;

namespace Followme.AspNet.Core.FastCommon.ThirdParty.GrpcServer
{
    public class ProtoMessageNameCompare : IComparer<string>
    {
        public int Compare(string x, string y)
        {
           return string.CompareOrdinal(x,y);
        }
    }
}