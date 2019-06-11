using System.Collections.Generic;

namespace Kadder
{
    public class GrpcClientExtension
    {
        public static IDictionary<string,GrpcClient> ClientDic = new Dictionary<string,GrpcClient>();
    }
}
