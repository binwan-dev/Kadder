using System.Reflection;
using System.Linq;
using System;
using System.Collections.Generic;
using Kadder.Grpc.Client.Options;
using Kadder.Utils;

namespace Kadder.Grpc.Client.AspNetCore
{
    public class ClientBuilder : KadderBuilder
    {
        public const string ConfigurationKeyName = "GrpcClient";

        public ClientBuilder()
        {
            Clients = new List<GrpcClient>();
        }

        public List<GrpcClient> Clients { get; set; }

        internal List<Type> ServicerTypes { get; set; }

        public ClientBuilder AddClient(GrpcClientOptions options)
        {
            foreach (var assemblyName in options.AssemblyNames)
                options.Assemblies.Add(Assembly.LoadFrom(assemblyName));

            var servicerTypes = ServicerHelper.GetServicerTypes(options.Assemblies);
            new GrpcClient(servicerTypes, options);
            Assemblies.AddRange(options.Assemblies);
            ServicerTypes.AddRange(servicerTypes);
            return this;
        }
    }
}
