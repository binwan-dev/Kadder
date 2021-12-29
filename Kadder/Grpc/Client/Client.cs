using System;
using System.Collections.Generic;
using Kadder.Grpc.Client.Options;

namespace Kadder.Grpc.Client
{
    public class Client
    {
        public Client(List<GrpcProxyer> proxyers, List<GrpcProxyerOptions> proxyerOptions)
        {
            Proxyers = proxyers;
            ProxyerOptions = proxyerOptions;
        }

        public List<GrpcProxyerOptions> ProxyerOptions { get; }

        public List<GrpcProxyer> Proxyers { get; }
    }
}