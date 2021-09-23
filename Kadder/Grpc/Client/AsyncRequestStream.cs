using Grpc.Core;
using Kadder.Streaming;

namespace Kadder.Grpc.Client
{
    public class AsyncRequestStream<TRequest> : IAsyncRequestStream<TRequest> where TRequest : class
    {
        public AsyncRequestStream()
        {
        }

        internal IAsyncStreamWriter<TRequest> StreamWriter{get;set;}

        public string Name{get;set;}
    }
}
