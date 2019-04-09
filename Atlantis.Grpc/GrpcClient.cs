using Atlantis.Common.CodeGeneration;
using Atlantis.Grpc.Utilies;
using Grpc.Core;

namespace Atlantis.Grpc
{
    public class GrpcClient
    {

        private static bool _ifloaded=false;
        private readonly CodeAssembly _codeAssembly;
        private readonly CodeBuilder _codeBuilder;
        private readonly GrpcOptions _options;
        private readonly Channel _channel;

        public GrpcClient(GrpcOptions options)
        {
            _options = options;

            var namespaces = $"Atlantis.Grpc.Client.Services";
            _codeBuilder = new CodeBuilder(namespaces, namespaces);
            _codeBuilder = GrpcClientBuilder.Instance
                .GenerateHandler(options, _codeBuilder)
                .AddAssemblyRefence(this.GetType().Assembly.Location);
            _codeAssembly = _codeBuilder.BuildAsync().Result;
            _channel = new Channel(
                _options.Host, _options.Port, ChannelCredentials.Insecure);
            _channel.ConnectAsync().Wait();

            if(!_ifloaded)
            {
                ObjectContainer.SetContainer(_options.ObjectContainer);
                ObjectContainer.Register<IBinarySerializer,ProtobufBinarySerializer>(LifeScope.Single);
            }
        }

        public T GetService<T>() where T : class
        {
            if (Cache<T>.cache != null)
            {
                return Cache<T>.cache;
            }

            var type = typeof(T);
            var fullName = $"{_codeBuilder.Namespace}.{type.Name.Remove(0, 1)}";
            var t = _codeAssembly.Assembly.CreateInstance(fullName) as T;
            if (t == null)
            {
                return t;
            }
            var serviceName = $"{_options.NamespaceName}.{_options.ServiceName}";
            var invoker = new GrpcClientInvoker<T>(_channel, serviceName);
            Cache<T>.cache = t;
            return t;
        }

        static class Cache<T>
        {
            public static T cache;
        }
    }
}
