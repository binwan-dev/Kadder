using System.Reflection;
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
            AssemblyNames = new List<string>();
        }

        public GrpcServerOptions Options { get; set; }

        internal IList<Type> GrpcServicerProxyers { get; set; }

        internal List<Type> Interceptors { get; set; }

        public List<string> AssemblyNames { get; set; }

        public GrpcServerBuilder AddInterceptor<T>()
        {
            Interceptors.Add(typeof(T));
            return this;
        }
    }
}
