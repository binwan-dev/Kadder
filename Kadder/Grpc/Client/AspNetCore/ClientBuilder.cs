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
        }

        public ClientBuilder AddClient(GrpcClientOptions options)
        {
            var servicerTypes = ServicerHelper.GetServicerTypes(options.Assemblies);
            new GrpcClient(servicerTypes, options);
            Assemblies.AddRange(options.Assemblies);
            return this;
        }
    }
}
