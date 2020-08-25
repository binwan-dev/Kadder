using System;
using System.Collections.Generic;
using Grpc.Core.Interceptors;
using Kadder.Utilies;

namespace Kadder
{
    public class GrpcClientBuilder
    {
        public GrpcClientBuilder()
        {
            Interceptors = new List<Type>();
            ClientMetadatas = new List<GrpcClientMetadata>();
            BinarySerializer = new ProtobufBinarySerializer();
            Strategy = new RoundRobinStrategy();
        }

        public IList<Type> Interceptors { get; }

        public static IServiceProvider ServiceProvider { get; set; }

        public IBinarySerializer BinarySerializer { get; set; }

        public IList<GrpcClientMetadata> ClientMetadatas { get; private set; }

        public IGrpcClientStrategy Strategy { get; set; }

        [Obsolete("Use RegClient(GrpcClientOptions) version")]
        public GrpcClientMetadata RegClient(GrpcOptions options)
        {
            return RegClient(new GrpcClientOptions(options));
        }

        public GrpcClientMetadata RegClient(GrpcClientOptions options)
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
        
        public GrpcClientBuilder UseTextJsonSerializer()
        {
            BinarySerializer = new TextJsonSerializer();
            return this;
        }
    }
}
