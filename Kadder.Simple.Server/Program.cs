using System;
using System.Reflection;
using System.Threading.Tasks;
using Grpc.Core;
using Kadder.Simple.Protocol.Servicers;
using Kadder.Simple.Server.Servicers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

namespace Kadder.Simple.Server
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder()
                .UseGrpcServer()
                .ConfigureServices(services =>
                {
                    // services.AddScoped<IPersonServicer, PersonServicer>();
                })
                .Build();

            host.StartGrpcServer();
            Console.WriteLine("Server is running...");
            await host.RunAsync();
        }
    }
}
