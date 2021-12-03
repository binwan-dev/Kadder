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
            var host = new Microsoft.Extensions.Hosting.HostBuilder()
                .UseGrpcServer((context, services, builder) =>
                {
                    builder.Assemblies.Add(typeof(IPersonServicer).Assembly);
                    builder.Options = new GrpcServerOptions();
                    builder.Options.PackageName = "Kadder.Servicer";
                    builder.Options.Ports.Add(new GrpcServerPort() { Port = 3002 });

                    services.AddScoped<IPersonServicer, PersonServicer>();
                }).Build();

            host.StartGrpcServer();
            Console.WriteLine("Server is running...");
            await host.RunAsync();
        }
    }
}
