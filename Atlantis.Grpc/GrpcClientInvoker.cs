using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Atlantis.Grpc.Utilies;
using Grpc.Core;

namespace Atlantis.Grpc
{
    public interface IGrpcClientInvoker
    {
        Task<TResponse> CallAsync<TRequest, TResponse>(
            TRequest request, string methodName)
            where TRequest : class
            where TResponse : class;
    }

    public class GrpcClientInvokerExtension
    {
        public static IDictionary<Type, IGrpcClientInvoker> TypeDic = new Dictionary<Type, IGrpcClientInvoker>();
    }

    public class GrpcClientInvoker<T> : ClientBase, IGrpcClientInvoker
    {
        private readonly IBinarySerializer _binarySerializer;
        private T _agent;
        private readonly Channel _channel;
        private readonly string _serviceName;

        public GrpcClientInvoker(Channel channel,string serviceName) : base(channel)
        {
            _binarySerializer = ObjectContainer.Resolve<IBinarySerializer>();
            _channel = channel;
            _serviceName=serviceName;

            if (!GrpcClientInvokerExtension.TypeDic.ContainsKey(typeof(T)))
            {
                GrpcClientInvokerExtension.TypeDic.Add(typeof(T), this);
            }
        }

        public T Agent
        {
            get
            {
                if (_agent == null)
                {
                    _agent = NewInstance();
                }
                return _agent;
            }
        }

        public async Task<TResponse> CallAsync<TRequest, TResponse>(
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
                _serviceName,
                methodName,
                requestMarshaller,
                responseMarshaller);

            var callOption = new CallOptions();

            var result = CallInvoker.AsyncUnaryCall<TRequest, TResponse>(
                method, null, callOption, request);
            
            return await result.ResponseAsync;
        }

        private T NewInstance()
        {
            return ObjectContainer.Resolve<T>();
        }
    }

}
