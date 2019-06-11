using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Atlantis.Common.CodeGeneration;
using Grpc.Core;
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
        private readonly IBinarySerializer _binarySerializer;
        private CallInvoker _grpcInvoker;

        public GrpcClient(
            GrpcOptions options,
            GrpcServiceCallBuilder serviceCallBuilder,
            IBinarySerializer binarySerializer)
        {
            _options = options;

            ID = Guid.NewGuid();
            GrpcServiceDic = new Dictionary<Type, Type>();
            var namespaces = $"Kadder.Client.Services";
            _codeBuilder = new CodeBuilder(namespaces, namespaces);
            var grpcServiceDic = serviceCallBuilder
                .GenerateHandler(_options, this, ref _codeBuilder);
            _codeAssembly = _codeBuilder.BuildAsync().Result;
            _binarySerializer = binarySerializer;

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
    }
}
