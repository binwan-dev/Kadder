using System;
using System.Threading;
using System.Threading.Tasks;
using Kadder.Grpc.Client;
using Kadder.Streaming;
using Kadder.Utilies;
using Kadder.Utils;

namespace Kadder.Grpc.Client
{
    public static class KadderGrpcClientExtension
    {
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
