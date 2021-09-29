using Grpc.Core;
using Kadder.Streaming;

namespace Kadder.Grpc.Client
{
    public class AsyncRequestStream<TRequest> : IAsyncRequestStream<TRequest> where TRequest : class
    {
        public AsyncRequestStream()
        {
        }

        internal IClientStreamWriter<TRequest> StreamWriter { get; set; }
    }
}
