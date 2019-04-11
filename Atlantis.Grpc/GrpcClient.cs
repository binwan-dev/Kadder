using System;
using System.Reflection;
using System.Threading.Tasks;
using Atlantis.Common.CodeGeneration;
using Atlantis.Grpc.Logging;
using Atlantis.Grpc.Utilies;
using Grpc.Core;

namespace Atlantis.Grpc
{
    public class GrpcClient
    {
        private static bool _ifloaded = false;
        private readonly CodeAssembly _codeAssembly;
        private readonly CodeBuilder _codeBuilder;
        private readonly GrpcOptions _options;
        private readonly IBinarySerializer _binarySerializer;
        private CallInvoker _grpcInvoker;

        public GrpcClient(GrpcOptions options)
        {
            _options = options;

            ID = Guid.NewGuid();
            var namespaces = $"Atlantis.Grpc.Client.Services";
            _codeBuilder = new CodeBuilder(namespaces, namespaces);
            _codeBuilder = GrpcClientBuilder.Instance
                .GenerateHandler(options, this, _codeBuilder);
            _codeAssembly = _codeBuilder.BuildAsync().Result;
            _binarySerializer = GrpcConfiguration.BinarySerializer;

            GrpcClientExtension.ClientDic.Add(ID.ToString(),this);

            if (!_ifloaded)
            {
                ObjectContainer.SetContainer(GrpcConfiguration.ObjectContainer);
                ObjectContainer.RegisterInstance(GrpcConfiguration.BinarySerializer);
                ObjectContainer.Register<ILoggerFactory, LoggerFactory>(LifeScope.Single);
            }
        }

        public Guid ID { get; }

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
            Cache<T>.cache = t;
            return t;
        }

        public virtual async Task<TResponse> CallAsync<TRequest, TResponse>(
            TRequest request, string methodName)
            where TRequest : class
            where TResponse : class
        {
            if (string.IsNullOrWhiteSpace(methodName))
            {
                throw new RpcException(new Status(StatusCode.Unknown, "No target!"));
            }

            var requestMarshaller = new Marshaller<TRequest>(
                _binarySerializer.Serialize,
                _binarySerializer.Deserialize<TRequest>);
            var responseMarshaller = new Marshaller<TResponse>(
                _binarySerializer.Serialize,
                _binarySerializer.Deserialize<TResponse>);
            var method = new Method<TRequest, TResponse>(
                MethodType.Unary,
                $"{_options.NamespaceName}.{_options.ServiceName}",
                methodName,
                requestMarshaller,
                responseMarshaller);

            var invoker = await GetInvokerAsync();
            var result = invoker.AsyncUnaryCall<TRequest, TResponse>(
                method, null, new CallOptions(), request);

            return await result.ResponseAsync;
        }

        protected virtual async Task<CallInvoker> GetInvokerAsync()
        {
            if (_grpcInvoker == null)
            {
                var channel = new Channel(
                    _options.Host, _options.Port, ChannelCredentials.Insecure);
                await channel.ConnectAsync();
                _grpcInvoker = new DefaultCallInvoker(channel);
            }
            return _grpcInvoker;
        }

        static class Cache<T>
        {
            public static T cache;
        }
    }
}
