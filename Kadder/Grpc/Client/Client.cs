using System;
using System.Collections.Generic;
using Kadder.Grpc.Client.Options;

namespace Kadder.Grpc.Client
{
    public class Client
    {
        public Client(List<Type> servicerTypes, List<GrpcProxyer> proxyers, List<GrpcProxyerOptions> proxyerOptions)
        {
            ServicerTypes = servicerTypes;
            Proxyers = proxyers;
            ProxyerOptions = proxyerOptions;
        }

        public List<GrpcProxyerOptions> ProxyerOptions { get; }

        public List<Type> ServicerTypes { get; }

        public List<GrpcProxyer> Proxyers { get; }
    }
}