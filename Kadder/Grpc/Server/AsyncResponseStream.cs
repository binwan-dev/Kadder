using System;
using Grpc.Core;
using Kadder.Streaming;

namespace Kadder.Grpc.Server
{
    public class AsyncResponseStream<T> : IAsyncResponseStream<T> where T : class
    {
        private readonly IServerStreamWriter<T> _writer;

        public AsyncResponseStream(IServerStreamWriter<T> writer)
        {
            _writer = writer;
        }

        public IServerStreamWriter<T> GrpcWriter => _writer;
    }
}
