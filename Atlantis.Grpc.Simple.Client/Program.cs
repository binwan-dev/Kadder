using System;
using Atlantis.Simple;
using Grpc.Core;

namespace Atlantis.Grpc.Simple.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            var channel = new Channel("127.0.0.1", 3002, ChannelCredentials.Insecure);

            channel.ConnectAsync().Wait();
            ClientBase<IAtlantisService> client = new ClientBase<IAtlantisService>();


            Console.WriteLine("Hello World!");
        }
    }
}
