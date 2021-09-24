using System;
using System.Collections.Generic;

namespace Kadder.Grpc.Server.AspNetCore
{
    public class GrpcServerBuilder : KadderBuilder
    {
        public GrpcServerBuilder()
        {
            GrpcServicerProxyers = new List<Type>();
            Interceptors = new List<Type>();
            Options = new GrpcServerOptions();
        }

        public GrpcServerOptions Options { get; set; }

        internal IList<Type> GrpcServicerProxyers { get; set; }

        public List<Type> Interceptors { get; set; }
    }
}
