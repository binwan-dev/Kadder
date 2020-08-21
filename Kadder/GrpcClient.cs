using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GenAssembly;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Kadder.Utilies;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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
        private ILogger<GrpcClient> _log;
        private DateTime _lastConnectedTime;
        private int _connecting = 0;
        private bool _isConnected=false;

        public GrpcClient(GrpcClientMetadata metadata, GrpcClientBuilder builder, GrpcServiceCallBuilder serviceCallBuilder)
        {
            _clientBuilder = builder;
            _metadata = metadata;
            _options = metadata.Options;
            ID = Guid.NewGuid();
            _lastConnectedTime = DateTime.Now;
            GrpcServiceDic = new Dictionary<Type, Type>();
            _oldMethodDic = new Dictionary<string, string>();
            var namespaces = $"Kadder.Client.Services";
            _codeBuilder = new CodeBuilder(namespaces, namespaces);

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

        protected ILogger<GrpcClient> Log
        {
            get
            {
                if (_log == null)
                {
                    _log = GrpcClientBuilder.ServiceProvider.GetService<ILogger<GrpcClient>>();
                }
                return _log;
            }
        }

        public virtual async Task<TResponse> CallAsync<TRequest, TResponse>(TRequest request, string methodName, string serviceName)
            where TRequest : class
            where TResponse : class
        {
            var name = $"{serviceName}{methodName}";
            try
            {
                var response = await DoCallAsync<TRequest, TResponse>(request, methodName, serviceName);
                return response;
            }
            catch (Exception ex)
            {
                RpcException rpcException;
                if (ex.EatException<RpcException>(out rpcException))
                {
                    throw rpcException;
                }
                throw ex;
            }
        }

        protected virtual async Task<TResponse> DoCallAsync<TRequest, TResponse>(TRequest request, string methodName, string serviceName)
            where TRequest : class
            where TResponse : class
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

            return await InvokeAsync(method, $"{_options.Host}:{_options.Port}", request);
        }

        private async Task<TResponse> InvokeAsync<TRequest, TResponse>(Method<TRequest, TResponse> method, string host, TRequest request)
            where TRequest : class
            where TResponse : class
        {
            try
            {
                var invoker = await GetInvokerAsync();
                var result = invoker.AsyncUnaryCall<TRequest, TResponse>(method, host, new CallOptions(), request);
                return await result.ResponseAsync;
            }
            catch (Exception ex)
            {
                RpcException rpcException;
                if (!ex.EatException<RpcException>(out rpcException) || rpcException.Status.StatusCode != StatusCode.Unavailable)
                {
                    throw ex;
                }

                _isConnected=false;
                await CreateChannelAsync();
                return await InvokeAsync(method, host, request);
            }
        }

        protected async Task<CallInvoker> GetInvokerAsync()
        {
            if (_grpcInvoker == null)
            {
                await CreateChannelAsync();

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
            return _grpcInvoker;
        }

        protected async virtual Task<Channel> CreateChannelAsync()
        {
            while(!(Interlocked.CompareExchange(ref _connecting, 1, 0) == 0))
            {
                System.Threading.Thread.Sleep(500);
                if(_isConnected)
                {
                    return _channel;
                }
            }

            Console.WriteLine("connect");
            try
            {
               
                _channel = new Channel(_options.Host, _options.Port, ChannelCredentials.Insecure);
                await _channel.ConnectAsync(DateTime.UtcNow.AddSeconds(_metadata.Options.ConnectSecondTimeout));
                _isConnected=true;
                return _channel;
            }
            catch(TaskCanceledException)
            {
                throw new RpcException(new Status(StatusCode.Aborted,""),$"Faild connect {_channel.Target}");
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                Interlocked.Exchange(ref _connecting, 0);
            }
        }

    }
}
