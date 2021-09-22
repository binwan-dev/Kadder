using System.Threading.Tasks;
using Kadder.Streaming;
using Kadder.Utilies;
using Kadder.Grpc.Server;
using System.Threading;
using System;

namespace Kadder.Simple.Server
{
    public interface IAnimalMessageServicer : IMessagingServicer
    {
        Task<HelloMessageResult> HelloAsync(HelloMessage message);

        Task DuplexAsync(IAsyncRequestStream<HelloMessage> request, IAsyncResponseStream<HelloMessageResult> response);

        Task ServerAsync(HelloMessage request, IAsyncResponseStream<HelloMessageResult> response);

        Task<HelloMessageResult> ClientAsync(IAsyncRequestStream<HelloMessage> request);

        Task HelloAsync();
    }

    public class AnimalMessageServicer : IAnimalMessageServicer
    {
        public async Task<HelloMessageResult> ClientAsync(IAsyncRequestStream<HelloMessage> request)
        {
            var token = new CancellationToken();
            while (await request.MoveNextAsync(token))
            {
                var req = request.GetCurrent();
                Console.WriteLine($"Requet: {req.Name} Type: {req.Type}");
            }

            return new HelloMessageResult() { Result = "Client stream result!" };
        }

        public Task DuplexAsync(IAsyncRequestStream<HelloMessage> request, IAsyncResponseStream<HelloMessageResult> response)
        {
            throw new System.NotImplementedException();
        }

        public Task<HelloMessageResult> HelloAsync(HelloMessage message)
        {
            var result = new HelloMessageResult() { Result = $"Hello, {message.Name}" };
            return Task.FromResult(result);
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
