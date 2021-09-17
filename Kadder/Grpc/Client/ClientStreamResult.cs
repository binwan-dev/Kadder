using System.Threading.Tasks;
using Kadder.Streaming;

namespace Kadder.Grpc.Client
{
    public class ClientStreamResult<TRequest,TResponse> where TRequest:class
    {
        public Task<TResponse> ResponseAsync{get; internal set;}

        public IAsyncRequestStream<TRequest> RequestStream{get; internal set;}
    }
}
