using System.Threading.Tasks;
using Kadder.Streaming;
using Kadder.Utilies;
using Kadder.Grpc.Server;
using System.Threading;
using System;
using Grpc.Core;

namespace Kadder.Simple.Server
{
    public interface IAnimalMessageServicer : IMessagingServicer
    {
        Task<HelloMessageResult> HelloAsync(HelloMessage message);

        Task DuplexAsync(IAsyncRequestStream<HelloMessage> request, IAsyncResponseStream<HelloMessageResult> response);

        Task ServerAsync(HelloMessage request, IAsyncResponseStream<HelloMessageResult> response);

        Task<HelloMessageResult> ClientAsync(IAsyncRequestStream<HelloMessage> request);

        Task HelloVoidAsync();
    }

    public class AnimalMessageServicer : IAnimalMessageServicer
    {
        public async Task<HelloMessageResult> ClientAsync(IAsyncRequestStream<HelloMessage> request)
        {
            var i = 0;
            var token = new CancellationToken();
            while (await request.MoveNextAsync(token))
            {
                // System.Threading.Thread.Sleep(2000);
                var req = request.GetCurrent();
                Console.WriteLine($"Requet: {req.Name} Type: {req.Type}");
                i++;
                // if (i == 4)
                //     throw new RpcException(new Status(StatusCode.Internal, "client stream error"));
            }

            return new HelloMessageResult() { Result = "Client stream result!" };
        }

        public async Task DuplexAsync(IAsyncRequestStream<HelloMessage> request, IAsyncResponseStream<HelloMessageResult> response)
        {
            var token = new CancellationToken();
            while (await request.MoveNextAsync(token))
            {
                var req = request.GetCurrent();
                Console.WriteLine($"Requet: {req.Name} Type: {req.Type}");
                await response.WriteAsync(new HelloMessageResult() { Result = "Server result Type: {req.Type}" });
            }
        }

        public Task<HelloMessageResult> HelloAsync(HelloMessage message)
        {
            System.Threading.Thread.Sleep(5000);
            var result = new HelloMessageResult() { Result = $"Server Result: {message.Name}" };
            Console.WriteLine(message.Name);
            return Task.FromResult(result);
        }

        public Task HelloVoidAsync()
        {
            // throw new RpcException(new Status(StatusCode.Internal, "sdfsfsdf"));
            // System.Threading.Thread.Sleep(5000);
            Console.WriteLine("Server void!");
            return HelloAsync(new HelloMessage());
        }

        public async Task ServerAsync(HelloMessage request, IAsyncResponseStream<HelloMessageResult> response)
        {
            Console.WriteLine($"Requet: {request.Name} Type: {request.Type}");
            for (var i = 0; i < 10; i++)
            {
                await response.WriteAsync(new HelloMessageResult() { Result = "Server result Type: {req.Type} Name: {request.Name}-{i}" });
            }
        }
    }
}
