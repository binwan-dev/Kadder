using Grpc.Core;
using Kadder.Messaging;
using Kadder.Streaming;

namespace Kadder.Grpc.Server
{
    public class GrpcMessagingContext : MessagingContext
    {
        private readonly ServerCallContext _serverCallContext;
        private IAsyncStream _asyncResponseStream;

        public GrpcMessagingContext(ServerCallContext serverCallContext)
        {
            _serverCallContext = serverCallContext;
        }

        public ServerCallContext ServerCallContext => _serverCallContext;

        public AsyncResponseStream<T> GetResponseStream<T>() where T : class => (AsyncResponseStream<T>)_asyncResponseStream;

        internal void SetResponseStream<T>(AsyncResponseStream<T> responseStream) where T : class
        {
            _asyncResponseStream = responseStream;
        }
    }
}
