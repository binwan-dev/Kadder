using Kadder.Streaming;
using Grpc.Core;

namespace Kadder.Grpc.Server
{
    public class AsyncRequestStream<T> : IAsyncRequestStream<T> where T : class
    {
        private readonly IAsyncStreamReader<T> _reader;

        public AsyncRequestStream(IAsyncStreamReader<T> reader)
        {
            _reader = reader;
        }

        public IAsyncStreamReader<T> GrpcReader => _reader;
    }
}
