using System;
using System.Threading.Tasks;
using Grpc.Core;
using Kadder.Utilies;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;

namespace Kadder
{
    public class GrpcClient
    {
        private readonly GrpcClientMetadata _metadata;
        private readonly IGrpcClientStrategy _strategy;
        public static IDictionary<string, GrpcClient> ClientDic = new Dictionary<string, GrpcClient>();

        public GrpcClient(GrpcClientMetadata metadata, IGrpcClientStrategy strategy)
        {
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _strategy = strategy ?? throw new ArgumentNullException(nameof(metadata));

            AddConnToStrategy();
        }

        public Guid ID => _metadata.ID;

        public virtual async Task<TResponse> CallAsync<TRequest, TResponse>(TRequest request, string methodName, string serviceName)
            where TRequest : class where TResponse : class
        {
            if (string.IsNullOrWhiteSpace(methodName)) throw new RpcException(new Status(StatusCode.Unknown, "No target!"));
            var serializer = GrpcClientBuilder.ServiceProvider.GetService<IBinarySerializer>();
            serviceName = $"{_metadata.Options.NamespaceName}.{serviceName}";

            var requestMarshaller = new Marshaller<TRequest>(serializer.Serialize, serializer.Deserialize<TRequest>);
            var responseMarshaller = new Marshaller<TResponse>(serializer.Serialize, serializer.Deserialize<TResponse>);

            var method = new Method<TRequest, TResponse>(MethodType.Unary, serviceName, methodName, requestMarshaller, responseMarshaller);

            return await InvokeAsync(method, request);
        }

        private async Task<TResponse> InvokeAsync<TRequest, TResponse>(Method<TRequest, TResponse> method, TRequest request)
            where TRequest : class
            where TResponse : class
        {
            var conn = _strategy.GetConn();
            try
            {
                var invoker = await conn.GetInvokerAsync();
                var result = invoker.AsyncUnaryCall<TRequest, TResponse>(method, conn.Host.ToString(), new CallOptions(), request);
                return await result.ResponseAsync;
            }
            catch (Exception ex)
            {
                if (ex.EatException<RpcException>(out RpcException rpcException) && rpcException.Status.StatusCode == StatusCode.Unavailable)
                {
                    _strategy.ConnectBroken(conn);
                    throw rpcException;
                }
                throw ex;
            }
        }

        private void AddConnToStrategy()
        {
            var hostArr = _metadata.Options.Host.Split(';');
            foreach (var host in hostArr)
            {
                _strategy.AddConn(new GrpcConnection(_metadata, RpcHost.Parse(host)));
            }
        }

    }
}
