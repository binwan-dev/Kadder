using System.Threading.Tasks;
using Kadder.Simple.Server;
using Kadder.Streaming;

namespace Kadder.Simple.Client
{
    public class AnimalMessageServicer : IAnimalMessageServicer
    {
        public Task<HelloMessageResult> ClientAsync(IAsyncRequestStream<HelloMessage> request)
        {
            throw new System.NotImplementedException();
        }

        public Task DuplexAsync(IAsyncRequestStream<HelloMessage> request, IAsyncResponseStream<HelloMessageResult> response)
        {
            throw new System.NotImplementedException();
        }

        public Task<HelloMessageResult> HelloAsync(HelloMessage message)
        {
            throw new System.NotImplementedException();
        }

        public Task HelloAsync()
        {
            throw new System.NotImplementedException();
        }

        public Task ServerAsync(HelloMessage request, IAsyncResponseStream<HelloMessageResult> response)
        {
            throw new System.NotImplementedException();
        }
    }
}
