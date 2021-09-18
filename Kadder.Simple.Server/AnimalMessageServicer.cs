using System.Threading.Tasks;
using Kadder.Streaming;
using Kadder.Utilies;

namespace Kadder.Simple.Server
{
    public interface IAnimalMessageServicer : IMessagingServicer
    {
        Task<HelloMessageResult> HelloAsync(HelloMessage message);

        Task DuplexAsync(IAsyncRequestStream<HelloMessage> request,IAsyncResponseStream<HelloMessageResult> response);

        Task ServerAsync(HelloMessage request,IAsyncResponseStream<HelloMessageResult> response);

        Task<HelloMessageResult> ClientAsync(IAsyncRequestStream<HelloMessage> request);

        Task HelloAsync();
    }

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
            return HelloAsync(new HelloMessage());
        }

        public Task ServerAsync(HelloMessage request, IAsyncResponseStream<HelloMessageResult> response)
        {
            throw new System.NotImplementedException();
        }
    }
}
