using System;
using System.Collections.Generic;
using Kadder.Grpc.Client.Options;

namespace Kadder.Grpc.Client.AspNetCore
{
    public class ClientBuilder : KadderBuilder
    {
        public ClientBuilder()
        {
            GrpcServicerProxyers = new List<Type>();
        }

        internal IList<GrpcClientOptions> Clients { get; set; }

        internal IList<Type> GrpcServicerProxyers { get; set; }

        public ClientBuilder AddClient(GrpcClientOptions options)
        {
            Clients.Add(options);
            return this;
        }
    }
}
