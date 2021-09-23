using Grpc.Core;
using Kadder.Streaming;

namespace Kadder.Grpc.Client
{
    public class AsyncResponseStream<TResponse> : IAsyncResponseStream<TResponse> where TResponse : class
    {
        internal IAsyncStreamReader<TResponse> StreamReader { get; set; }
    }
}
