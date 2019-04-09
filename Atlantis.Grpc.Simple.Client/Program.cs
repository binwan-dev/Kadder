using System;
using System.Reflection;
using Atlantis.Grpc.Simple.Server;
using Atlantis.Grpc.Utilies;
// using Atlantis.Simple;
using Grpc.Core;
// using static Atlantis.Simple.AtlantisService;

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
                PackageName="Atlantis.Simple",
                ServiceName="AtlantisService",
                ScanAssemblies=new Assembly[]
                {
                    typeof(IPersonMessageServicer).Assembly
                }
            };
            var client=new GrpcClient(options);
            var servicer=client.GetService<IPersonMessageServicer>();
            var message=new HelloMessage(){Name="DotNet"};
            var result=servicer.HelloAsync(message).Result;
            
            
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
            Console.WriteLine(result.Result);
        }
    }
}
