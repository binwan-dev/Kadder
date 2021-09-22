using System;
using System.Collections.Generic;
using Kadder.Grpc.Client.Options;
using Kadder.Utils;

namespace Kadder.Grpc.Client.AspNetCore
{
    public class ClientBuilder : KadderBuilder
    {
        public ClientBuilder()
        {
            GrpcServicerProxyers = new List<Type>();
        }

        internal IList<Type> GrpcServicerProxyers { get; set; }

        public ClientBuilder AddClient(GrpcClientOptions options)
        {
            var servicerTypes = ServicerHelper.GetServicerTypes(options.Assemblies);
            new GrpcClient(servicerTypes, options);
            return this;
        }
    }
}
