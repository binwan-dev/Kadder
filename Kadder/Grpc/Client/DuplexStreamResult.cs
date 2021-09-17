using Kadder.Streaming;

namespace Kadder.Grpc.Client
{
    public class DuplexStreamResult<TRequest, TResponse> where TRequest : class where TResponse : class
    {
        public IAsyncRequestStream<TRequest> RequestStream { get; internal set; }

        public IAsyncResponseStream<TResponse> ResponseStream { get; internal set; }
    }
}
