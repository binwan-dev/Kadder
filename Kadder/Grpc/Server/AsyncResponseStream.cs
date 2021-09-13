using System;
using Grpc.Core;
using Kadder.Streaming;

namespace Kadder.Grpc.Server
{
    public class AsyncResponseStream<T> : IAsyncResponseStream<T> where T : class
    {
        private readonly IAsyncStreamWriter<T> _writer;

        public AsyncResponseStream(IAsyncStreamWriter<T> writer)
        {
            _writer = writer;
        }

        public IAsyncStreamWriter<T> GrpcWriter => _writer;
    }
}
