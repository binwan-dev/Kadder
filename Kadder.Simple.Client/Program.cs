using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Fnlinker.AIService.Protocol.Entities.Ocr;
using Fnlinker.AIService.Protocol.Services;
using Kadder;
using Kadder.Simple.Client;
using Kadder.Simple.Server;
using Kadder.Utilies;
using Microsoft.Extensions.DependencyInjection;
using ProtoBuf;

namespace Atlantis.Grpc.Simple.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            var options=new GrpcOptions()
            {
                Host="127.0.0.1",
                Port=3002,
                NamespaceName="Atlantis.Simple",
                ServiceName="AtlantisService",
                ScanAssemblies=new string[]
                {
                    typeof(IPersonMessageServicer).Assembly.FullName
                }
            };

            IServiceCollection services=new ServiceCollection();
            services.AddLogging();
            services.AddKadderGrpcClient(builder=>
            {
                builder.RegClient(options);
                builder.RegShareInterceptor<Kadder.Simple.Client.LoggerInterceptor>();
            });

            var provider = services.BuildServiceProvider();
            provider.ApplyKadderGrpcClient();

            TestInterceptor(provider);

            Console.ReadLine();
            
            // var channel = new Channel("127.0.0.1", 3002, ChannelCredentials.Insecure);

            // channel.ConnectAsync().Wait();

            // AtlantisServiceClient client=new AtlantisServiceClient(channel);
            // 
            // var result= client.Hello(message);

            // // var serailizer=new ProtobufBinarySerializer();
            // // var s=serailizer.Serialize(message);

            // // foreach(var b in s)
            // // {
            // //     Console.Write($" {b}");
            // }
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
            var resuslt = servicer.HelloAsync();
            resuslt = servicer.HelloAsync();
            // Console.WriteLine(resuslt.Result);
        }
        
    }
}
