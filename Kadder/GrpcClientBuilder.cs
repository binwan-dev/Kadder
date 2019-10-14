using System;
using System.Collections.Generic;
using Grpc.Core.Interceptors;
using Kadder.Utilies;
using Microsoft.Extensions.Options;

namespace Kadder
{
    public class GrpcClientBuilder
    {
        internal IList<Type> Interceptors { get; }

        public static IServiceProvider ServiceProvider { get; set; }

        public IBinarySerializer BinarySerializer { get; set; }

        internal IList<GrpcClientMetadata> ClientMetadatas { get; private set; }

        public GrpcClientBuilder()
        {
            Interceptors = new List<Type>();
            ClientMetadatas = new List<GrpcClientMetadata>();
        }

        public GrpcClientMetadata RegClient(GrpcOptions options)
        {
            var metadata = new GrpcClientMetadata(options);
            ClientMetadatas.Add(metadata);
            return metadata;
        }

        public GrpcClientBuilder RegShareInterceptor<T>() where T : Interceptor
        {
            Interceptors.Add(typeof(T));
            return this;
        }
    }
}
