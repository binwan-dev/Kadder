using System;
using System.Threading.Tasks;
using Kadder.Grpc.Client;
using Kadder.Streaming;
using Kadder.Utilies;

namespace System
{
    public static class KadderGrpcClientExtension
    {
        public static ClientStreamResult<TRequest, TResponse> ClientStreamAsync<TRequest, TResponse>(this IMessagingServicer servicer, Func<IAsyncRequestStream<TRequest>, Task<TResponse>> method) where TRequest : class
        {
            Console.WriteLine(method.Method.Name);
            throw new NotImplementedException();
        }

        public static IAsyncResponseStream<TResponse> ServerStreamAsync<TRequest,TResponse>(this IMessagingServicer servicer,Func<TRequest,IAsyncResponseStream<TResponse>,Task> method,TRequest request)where TResponse :class
        {
            throw new NotImplementedException();
        }

        public static DuplexStreamResult<TRequest, TResponse> DuplexStreamAsync<TRequest, TResponse>(this IMessagingServicer servicer, Func<IAsyncRequestStream<TRequest>, IAsyncResponseStream<TResponse>, Task> method) where TRequest : class where TResponse : class
        {
            throw new NotImplementedException();
        }
    }
}
