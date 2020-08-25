using System;
using System.Threading.Tasks;
using Grpc.Core;
using Kadder.Utilies;
using System.Collections.Generic;

namespace Kadder
{
    public class GrpcClient
    {
        private readonly GrpcClientMetadata _metadata;
        private readonly IGrpcClientStrategy _strategy;
        private readonly IBinarySerializer _serializer;
        public static IDictionary<string, GrpcClient> ClientDic = new Dictionary<string, GrpcClient>();

        public GrpcClient(GrpcClientMetadata metadata, GrpcClientBuilder builder)
        {
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _strategy = builder.Strategy ?? throw new ArgumentNullException(nameof(builder.Strategy));
            _serializer = metadata.Serializer == null ? builder.BinarySerializer : metadata.Serializer;

            AddConnToStrategy();
        }

        public Guid ID => _metadata.ID;

        public virtual async Task<TResponse> CallAsync<TRequest, TResponse>(TRequest request, string methodName, string serviceName)
            where TRequest : class where TResponse : class
        {
            if (string.IsNullOrWhiteSpace(methodName)) throw new RpcException(new Status(StatusCode.Unknown, "No target!"));
            serviceName = $"{_metadata.Options.NamespaceName}.{serviceName}";

            var requestMarshaller = new Marshaller<TRequest>(_serializer.Serialize, _serializer.Deserialize<TRequest>);
            var responseMarshaller = new Marshaller<TResponse>(_serializer.Serialize, _serializer.Deserialize<TResponse>);

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
                    conn.ConnectionBrokenAsync(_strategy);
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
