using System;
using System.Collections.Generic;

namespace Kadder.Grpc.Server.AspNetCore
{
    public class GrpcServerBuilder : KadderBuilder
    {
        public GrpcServerBuilder()
        {
            GrpcServicerProxyers = new List<Type>();
        }

        public GrpcServerOptions GrpcServerOptions { get; set; }

        internal IList<Type> GrpcServicerProxyers { get; set; }
    }
}
