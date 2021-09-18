using Kadder.Streaming;

namespace Kadder.Grpc.Client
{
    public class StreamMessage
    {
        public static IAsyncRequestStream<TRequest> CreateRequest<TRequest>() where TRequest : class
        {
            var stream= new AsyncRequestStream<TRequest>();
            stream.Name="ss";
            return stream;
        }

        public static IAsyncResponseStream<TResponse> CreateResponse<TResponse>() where TResponse : class
        {
            return new AsyncResponseStream<TResponse>();
        }
    }
}
