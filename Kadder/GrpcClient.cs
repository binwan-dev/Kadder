using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Atlantis.Common.CodeGeneration;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Kadder.Utilies;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Kadder
{
    public class GrpcClient
    {
        private readonly CodeAssembly _codeAssembly;
        private readonly CodeBuilder _codeBuilder;
        private readonly GrpcOptions _options;
        private CallInvoker _grpcInvoker;
        private readonly GrpcClientBuilder _clientBuilder;
        private readonly GrpcClientMetadata _metadata;
        private Channel _channel;

        public GrpcClient(GrpcClientMetadata metadata, GrpcClientBuilder builder, GrpcServiceCallBuilder serviceCallBuilder)
        {
            _clientBuilder = builder;
            _metadata = metadata;
            _options = metadata.Options;

            ID = Guid.NewGuid();
            GrpcServiceDic = new Dictionary<Type, Type>();
            var namespaces = $"Kadder.Client.Services";
            _codeBuilder = new CodeBuilder(namespaces, namespaces);

            var grpcServiceDic = serviceCallBuilder.GenerateHandler(_options, this, ref _codeBuilder);
            _codeAssembly = _codeBuilder.BuildAsync().Result;
            GrpcClientExtension.ClientDic.Add(ID.ToString(), this);
            foreach (var item in grpcServiceDic)
            {
                var type = _codeAssembly.Assembly.GetType(item.Value);
                GrpcServiceDic.Add(item.Key, type);
            }
        }

        public Guid ID { get; }

        internal IDictionary<Type, Type> GrpcServiceDic { get; private set; }

        public T GetService<T>() where T : class
        {
            return GrpcClientBuilder.ServiceProvider.GetService<T>();
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

            var serializer = GrpcClientBuilder.ServiceProvider.GetService<IBinarySerializer>();

            var requestMarshaller = new Marshaller<TRequest>(
                serializer.Serialize,
                serializer.Deserialize<TRequest>);
            var responseMarshaller = new Marshaller<TResponse>(
                serializer.Serialize,
                serializer.Deserialize<TResponse>);
            var method = new Method<TRequest, TResponse>(
                MethodType.Unary,
                $"{_options.NamespaceName}.{_options.ServiceName}",
                methodName,
                requestMarshaller,
                responseMarshaller);
            
            var invoker = await GetInvokerAsync();
            var result = invoker.AsyncUnaryCall<TRequest, TResponse>(
                method, $"{_options.Host}:{_options.Port}", new CallOptions(), request);

            return await result.ResponseAsync;
        }

        protected virtual async Task<CallInvoker> GetInvokerAsync()
        {
            if (_grpcInvoker == null)
            {
                _channel = new Channel(_options.Host, _options.Port, ChannelCredentials.Insecure);
                await _channel.ConnectAsync();
                _grpcInvoker = new DefaultCallInvoker(_channel);

                foreach (var interceptorType in _clientBuilder.Interceptors)
                {
                    var interceptor = (Interceptor) GrpcClientBuilder.ServiceProvider.GetService(interceptorType);
                    _grpcInvoker = _grpcInvoker.Intercept(interceptor);
                }

                foreach (var interceptorType in _metadata.PrivateInterceptors)
                {
                    var interceptor = (Interceptor)GrpcClientBuilder.ServiceProvider.GetService(interceptorType);
                    _grpcInvoker = _grpcInvoker.Intercept(interceptor);
                }
            }
            if (_channel.State != ChannelState.Ready)
            {
                await _channel.ConnectAsync();
            }
            return _grpcInvoker;
        }
    }
}
