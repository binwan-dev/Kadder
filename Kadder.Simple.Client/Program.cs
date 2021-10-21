using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Kadder.Simple.Server;
using Microsoft.Extensions.DependencyInjection;
using Kadder.Grpc.Client;
using Kadder.Streaming;
using Kadder.Grpc.Client.Options;
using Grpc.Core;
using System.Threading;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;

namespace Atlantis.Grpc.Simple.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            Test().Wait();
        }

        static async Task Test()
        {
            var host = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder()
                .UseEnvironment("Development")
                .UseGrpcClient((context, servicers, builder) =>
                {
                    builder.ClientOptions[0].AddInterceptor<Kadder.Simple.Client.LoggerInterceptor>();
                    Console.WriteLine(builder.ClientOptions.Count);
                })
                .Build();

            var provider = host.Services;
            var animalMessageServicer = provider.GetService<IAnimalMessageServicer>();

            await TestNormal(animalMessageServicer);
            // TestConcurrent(animalMessageServicer);
        }

        static async Task TestNormal(IAnimalMessageServicer animalMessageServicer)
        {
            // 1 unary & no parameter
            await animalMessageServicer.HelloVoidAsync();

            // 2 unary
            var request = new HelloMessage() { Name = "Kadder" };
            var result = await animalMessageServicer.HelloAsync(request);
            Console.WriteLine(result.Result);

            // 3 client stream
            var requestStream = new AsyncRequestStream<HelloMessage>();
            var clientResult = animalMessageServicer.ClientAsync(requestStream);
            for (var i = 0; i < 10; i++)
            {
                Console.WriteLine($"Send client stream no {i}");
                await requestStream.WriteAsync(new HelloMessage() { Name = $"Kadder.ClientStream.{i}" });
            }
            await requestStream.CompleteAsync();
            result = await clientResult;
            Console.WriteLine(result.Result);

            // 4 server stream
            Console.WriteLine("\nServer stream begin: ");
            var responseStream = new AsyncResponseStream<HelloMessageResult>();
            await animalMessageServicer.ServerAsync(request, responseStream);
            var cancelToken = new CancellationToken();
            while (await responseStream.MoveNextAsync(cancelToken))
            {
                result = responseStream.GetCurrent();
                Console.WriteLine(result.Result);
            }
            Console.WriteLine("\nServer stream  ");

            // 5 duplex stream
            requestStream = new AsyncRequestStream<HelloMessage>();
            responseStream = new AsyncResponseStream<HelloMessageResult>();
            await animalMessageServicer.DuplexAsync(requestStream, responseStream);
            for (var i = 0; i < 10; i++)
            {
                await requestStream.WriteAsync(new HelloMessage() { Name = $"Kadder.ClientStream.{i}" });
                await responseStream.MoveNextAsync(cancelToken);
                Console.WriteLine(result.Result);
            }
        }

        static void TestConcurrent(IAnimalMessageServicer animalMessageServicer)
        {
            var task1Req = new HelloMessage() { Name = "Task1" };
            var task2Req = new HelloMessage() { Name = "Task2" };

            var task1 = animalMessageServicer.HelloAsync(task1Req);
            var task2 = animalMessageServicer.HelloAsync(task2Req);
            var task1Stream = SendClientStream(animalMessageServicer);

            var tasks = new Task<HelloMessageResult>[] { task1, task2, task1Stream };
            var result = Task.WaitAny(tasks);
            // var result = await SendClientStream(animalMessageServicer);
            Console.WriteLine(JsonConvert.SerializeObject(tasks[result].Result));
        }

        static async Task<HelloMessageResult> SendClientStream(IAnimalMessageServicer animalMessageServicer)
        {
            var requestStream = new AsyncRequestStream<HelloMessage>();
            var result = animalMessageServicer.ClientAsync(requestStream);
            for (var i = 0; i < 10; i++)
            {
                Console.WriteLine($"Send client stream no {i}");
                await requestStream.WriteAsync(new HelloMessage() { Name = $"Kadder.ClientStream.{i}" });
            }
            await requestStream.CompleteAsync();
            return await result;
        }

        // static void TestAI(ServiceProvider provider)
        // {
        //     var service = provider.GetService<IOcrService>();
        //     var request = new ImageUrlOcrRequest()
        //     {
        //         Url = "https://resapi.neobai.com/previews/1195351096063823872.jpg"
        //     };
        //     var response = service.GetImageTagsByUrlAsync(request);
        // }

        static void TestNumber(ServiceProvider provider)
        {
            var i = 0;
            var servicer = provider.GetService<INumberMessageServicer>();
            while (true)
            {
                servicer.PrintAsync(new NumberMessage { Number = i++ }).Wait();
            }
        }

        static void TestParallel(ServiceProvider provider)
        {
            try
            {
                var servicer = provider.GetService<IPersonMessageServicer>();
                var message = new HelloMessage() { Name = "DotNet" };
                var stopwatch = new Stopwatch();
                var resuslt = servicer.HelloAsync();
                stopwatch.Start();
                System.Threading.Tasks.Parallel.For(0, 10000, i =>
                {
                    var result = servicer.HelloAsync();
                    Console.WriteLine(result);
                });
                stopwatch.Stop();
                Console.WriteLine(stopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        static void TestInterceptor(ServiceProvider provider)
        {
            var servicer = provider.GetService<IPersonMessageServicer>();
            var message = new HelloMessage() { Name = "test interceptor" };

            for (var i = 0; i < 10; i++)
            {
                Task.Run(() =>
                {
                    while (true)
                    {
                        try
                        {
                            var resuslt = servicer.HelloAsync().Result;
                            Console.WriteLine(resuslt.Result);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.ToString());
                        }

                        // Console.WriteLine(resuslt.Result);
                        System.Threading.Thread.Sleep(1000);
                    }

                });
            }
        }

    }
}
