using System.Reflection;
using System;
using System.Collections.Generic;
using Grpc.Core.Interceptors;

namespace Kadder.Grpc.Server.AspNetCore
{
    public class GrpcServerBuilder : KadderBuilder
    {
        public GrpcServerBuilder()
        {
            GrpcServicerProxyers = new List<Type>();
            Interceptors = new List<Type>();
            Options = new GrpcServerOptions();
            AssemblyNames = new List<string>();
        }

        public GrpcServerOptions Options { get; set; }

        internal IList<Type> GrpcServicerProxyers { get; set; }

        internal List<Type> Interceptors { get; set; }

        public List<string> AssemblyNames { get; set; }

        public GrpcServerBuilder AddInterceptor<Interceptor>()
        {
            Interceptors.Add(typeof(Interceptor));
            return this;
        }
    }
}
