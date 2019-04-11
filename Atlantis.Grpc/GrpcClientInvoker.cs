using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Atlantis.Grpc.Utilies;
using Grpc.Core;

namespace Atlantis.Grpc
{
    public class GrpcClientExtension
    {
        public static IDictionary<string,GrpcClient> ClientDic = new Dictionary<string,GrpcClient>();
    }
}
