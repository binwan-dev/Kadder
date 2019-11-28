using Atlantis.Common.CodeGeneration;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Grpc.Core.Logging;
using Kadder.Utilies;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ILogger = Microsoft.Extensions.Logging.ILogger;

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
        private readonly IDictionary<string, string> _oldMethodDic;
        private readonly Lazy<ILogger<GrpcClient>> _log;

        public GrpcClient(
            GrpcClientMetadata metadata, GrpcClientBuilder builder, GrpcServiceCallBuilder serviceCallBuilder)
        {
            _clientBuilder = builder;
            _metadata = metadata;
            _options = metadata.Options;
            ID = Guid.NewGuid();
            GrpcServiceDic = new Dictionary<Type, Type>();
            _oldMethodDic = new Dictionary<string, string>();
            var namespaces = $"Kadder.Client.Services";
            _codeBuilder = new CodeBuilder(namespaces, namespaces);
            _log = new Lazy<ILogger<GrpcClient>>(() => GrpcClientBuilder.ServiceProvider.GetService<ILogger<GrpcClient>>());

            var grpcServiceDic = serviceCallBuilder.GenerateHandler(_options, this, ref _codeBuilder);
            _codeAssembly = _codeBuilder.BuildAsync().Result;
            GrpcClientExtension.ClientDic.Add(ID.ToString(), this);
            foreach (var keyValuePair in grpcServiceDic)
            {
                var type = _codeAssembly.Assembly.GetType(keyValuePair.Value);
                GrpcServiceDic.Add(keyValuePair.Key, type);
            }
        }

        public Guid ID { get; }

        internal IDictionary<Type, Type> GrpcServiceDic { get; private set; }

        public T GetService<T>() where T : class
        {
            return GrpcClientBuilder.ServiceProvider.GetService<T>();
        }

        public virtual async Task<TResponse> CallAsync<TRequest, TResponse>(
            TRequest request, string methodName, string serviceName)
            where TRequest : class
            where TResponse : class
        {
            var name = $"{serviceName}{methodName}";
            try
            {
                if (_oldMethodDic.ContainsKey(name))
                {
                    _log.Value.LogWarning($"ServiceCall has call old version. Name[{name}]");
                    var response = await CallForOldVersionAsync<TRequest, TResponse>(request, methodName);
                    return response;
                }
                var response1 = await DoCallAsync<TRequest, TResponse>(request, methodName, serviceName);
                return response1;
            }
            catch (Exception ex)
            {
                RpcException rpcException;
                if (!ex.EatException<RpcException>(out rpcException) || rpcException.Status.StatusCode != StatusCode.Unimplemented)
                {
                    throw ex;
                }
                _log.Value.LogWarning($"ServiceCall has call old version. Name[{name}]");
                var response = await CallForOldVersionAsync<TRequest, TResponse>(request, methodName);
                if (!_oldMethodDic.Keys.Contains(name))
                {
                    _oldMethodDic.Add(name, name);
                }
                return response;
            }
        }
        
        /// <summary>
        /// Support old version 0.0.6 and before version
        /// </summary>
        /// <param name="request"></param>
        /// <param name="methodName"></param>
        /// <typeparam name="TRequest"></typeparam>
        /// <typeparam name="TResponse"></typeparam>
        /// <returns></returns>
        public virtual Task<TResponse> CallForOldVersionAsync<TRequest, TResponse>(
            TRequest request, string methodName) where TRequest : class where TResponse : class
        {
            return DoCallAsync<TRequest, TResponse>(request, methodName, _options.ServiceName);
        }

        protected virtual async Task<TResponse> DoCallAsync<TRequest, TResponse>(
            TRequest request, string methodName, string serviceName) where TRequest : class where TResponse : class
        {
            if (string.IsNullOrWhiteSpace(methodName))
            {
                throw new RpcException(new Status(StatusCode.Unknown, "No target!"));
            }
            var serializer = GrpcClientBuilder.ServiceProvider.GetService<IBinarySerializer>();
            serviceName = $"{_options.NamespaceName}.{serviceName}";

            var requestMarshaller = new Marshaller<TRequest>(serializer.Serialize, serializer.Deserialize<TRequest>);
            var responseMarshaller = new Marshaller<TResponse>(serializer.Serialize, serializer.Deserialize<TResponse>);
            
            var method = new Method<TRequest, TResponse>(
                MethodType.Unary, serviceName, methodName, requestMarshaller, responseMarshaller);

            var invoker = await GetInvokerAsync();
            var result = invoker.AsyncUnaryCall<TRequest, TResponse>(method, $"{_options.Host}:{_options.Port}", new CallOptions(), request);
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
                    var interceptor = (Interceptor)GrpcClientBuilder.ServiceProvider.GetService(interceptorType);
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
                _grpcInvoker = new DefaultCallInvoker(_channel);
            }
            return _grpcInvoker;
        }
    }
}
