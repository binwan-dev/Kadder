using System;
using System.Collections.Generic;
using Grpc.Core.Interceptors;
using Kadder.Middlewares;
using Kadder.Utilies;
using Microsoft.Extensions.Options;

namespace Kadder
{
    public class GrpcServerBuilder
    {
        public GrpcServerBuilder()
        {
            Middlewares = new List<Type>();
            Interceptors = new List<Type>();
            Services = new List<Type>();
            BinarySerializer = new ProtobufBinarySerializer();
        }

        public GrpcServerOptions Options { get; set; }

        public IBinarySerializer BinarySerializer { get; set; }

        internal IList<Type> Middlewares { get; private set; }

        internal IList<Type> Interceptors { get; private set; }

        internal IList<Type> Services { get; private set; }

        public static IServiceProvider ServiceProvider { get; set; }

        public GrpcServerBuilder AddMiddleware<T>() where T : GrpcMiddlewareBase
        {
            Middlewares.Add(typeof(T));
            return this;
        }

        public GrpcServerBuilder AddInterceptor<T>() where T : Interceptor
        {
            Interceptors.Add(typeof(T));
            return this;
        }

        public GrpcServerBuilder UseTextJsonSerializer()
        {
            BinarySerializer = new TextJsonSerializer();
            return this;
        }

    }
}
