using System;
using System.Threading;
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

        public static Task WriteAsync<T>(this IAsyncRequestStream<T> requestStream, T message) where T : class
        {
            if (!(requestStream is AsyncRequestStream<T> grpcRequestStream))
                throw new InvalidCastException("The stream is not grpc stream!");

            return grpcRequestStream.StreamWriter.WriteAsync(message);
        }

        public static T GetCurrent<T>(this IAsyncResponseStream<T> responseStream) where T : class
        {
            if (!(responseStream is AsyncResponseStream<T> grpcResponseStream))
                throw new InvalidCastException("The stream is not grpc stream!");

            return grpcResponseStream.StreamReader.Current;
        }

        public static Task<bool> MoveNextAsync<T>(this IAsyncResponseStream<T> responseStream, CancellationToken cancellationToken) where T : class
        {
            if (!(responseStream is AsyncResponseStream<T> grpcResponseStream))
                throw new InvalidCastException("The stream is not grpc stream!");

            return grpcResponseStream.StreamReader.MoveNext(cancellationToken);
        }
    }
}
