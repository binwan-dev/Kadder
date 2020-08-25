using GenAssembly;
using Grpc.Core.Interceptors;
using Kadder.Utilies;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kadder
{
    public class GrpcClientMetadata
    {
        public GrpcClientMetadata(GrpcClientOptions options)
        {
            ID = Guid.NewGuid();
            Options = options;
            PrivateInterceptors = new List<Type>();
            GenerationCallTypes = generateCallTypes();
        }

        public IList<Type> PrivateInterceptors { get; set; }

        internal IList<Type> PublicInterceptors { get; set; }

        public GrpcClientOptions Options { get; set; }

        internal Guid ID { get; set; }

        internal IDictionary<Type, Type> GenerationCallTypes { get; set; }

        public IBinarySerializer Serializer { get; set; }

        public GrpcClientMetadata RegInterceptor<T>() where T : Interceptor
        {
            PrivateInterceptors.Add(typeof(T));
            return this;
        }

        public GrpcClientMetadata RegSerializer(IBinarySerializer serializer)
        {
            Serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            return this;
        }

        private IDictionary<Type, Type> generateCallTypes()
        {
            var codeBuilder = new CodeBuilder(Options.NamespaceName, Options.NamespaceName);
            var typeDic = GrpcServiceCallBuilder.GenerateHandler(Options, this, ref codeBuilder);
            var assembies = codeBuilder.BuildAsync().Result;

            var callTypes = new Dictionary<Type, Type>();
            foreach (var item in typeDic)
            {
                callTypes.Add(item.Key, assembies.Assembly.GetType(item.Value));
            }
            return callTypes;
        }
    }
}
