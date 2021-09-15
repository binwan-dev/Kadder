using System;
using System.Threading;
using System.Threading.Tasks;
using Kadder.Grpc.Server;
using Kadder.Streaming;

namespace Kadder.GrpcServer
{
    public static class ServerStreamExtension
    {
        public static Task WriteAsync<T>(this IAsyncResponseStream<T> serverStream, T message) where T : class
        {
            if (!(serverStream is AsyncResponseStream<T> grpcServerStream))
                throw new InvalidCastException("The stream is not grpc stream!");

            return grpcServerStream.GrpcWriter.WriteAsync(message);
        }

        public static T GetCurrent<T>(this IAsyncRequestStream<T> serverStream) where T : class
        {
            if (!(serverStream is AsyncRequestStream<T> grpcServerStream))
                throw new InvalidCastException("The stream is not grpc stream!");

            return grpcServerStream.GrpcReader.Current;
        }

        public static Task<bool> MoveNextAsync<T>(this IAsyncRequestStream<T> serverStream, CancellationToken cancellationToken) where T : class
        {
            if (!(serverStream is AsyncRequestStream<T> grpcServerStream))
                throw new InvalidCastException("The stream is not grpc stream!");

            return grpcServerStream.GrpcReader.MoveNext(cancellationToken);
        }
    }
}
