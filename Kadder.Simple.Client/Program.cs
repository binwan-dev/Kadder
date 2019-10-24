using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Kadder;
using Kadder.Simple.Client;
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
                builder.RegShareInterceptor<LoggerInterceptor>();
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

        static void TestParallel(ServiceProvider provider)
        {
            try
            {
                var servicer = provider.GetService<IPersonMessageServicer>();
                var message = new HelloMessage() { Name = "DotNet" };
                var stopwatch = new Stopwatch();
                var resuslt = servicer.HelloAsync(message).Result;
                stopwatch.Start();
                System.Threading.Tasks.Parallel.For(0, 10000, i =>
                {
                    var result = servicer.HelloAsync(message).Result;
                    Console.WriteLine(result.Result);
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
            var resuslt = servicer.HelloAsync(message).Result;
            resuslt = servicer.HelloAsync(message).Result;
            Console.WriteLine(resuslt.Result);
        }
        
    }

    public interface IPersonMessageServicer:IMessagingServicer
    {
        Task<HelloMessageResult> HelloAsync(HelloMessage message);
    }

    public class PersonMessageServicer : IPersonMessageServicer
    {
        public Task<HelloMessageResult> HelloAsync(HelloMessage message)
        {
            var result = $"Hello, {message.Name}";
            return Task.FromResult(new HelloMessageResult()
            {
                Result = result
            });
        }
    }

    [ProtoContract(ImplicitFields=ImplicitFields.AllPublic)]
    public class HelloMessage : BaseMessage
    {
        public string Name { get; set; }
    }

    [ProtoContract(ImplicitFields=ImplicitFields.AllPublic)]
    public class HelloMessageResult : MessageResult
    {
        public string Result { get; set; }
    }
}
