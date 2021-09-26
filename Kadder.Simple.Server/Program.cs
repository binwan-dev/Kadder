using System;
using System.Reflection;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Kadder.Simple.Server
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine(Environment.CurrentDirectory);

            var host = new Microsoft.Extensions.Hosting.HostBuilder()
                .ConfigureServices((context, services) =>
                {
                    services.AddLogging();
                    services.UseGrpcServer(builder =>
                    {
                        builder.Assemblies.Add(Assembly.GetExecutingAssembly());
                        builder.Options = new GrpcServerOptions();
                        builder.Options.PackageName = "Atlantis.Simple";
                        builder.Options.Ports.Add(new ServerPort("0.0.0.0", 3001, ServerCredentials.Insecure));
                    });
                    services.AddScoped<IPersonMessageServicer, PersonMessageServicer>();
                    services.AddScoped<IAnimalMessageServicer, AnimalMessageServicer>();
                    services.AddScoped<INumberMessageServicer, NumberMessageServicer>();
                    services.AddScoped<ImplServicer>();
                    services.AddScoped<AttributeServicer>();
                    services.AddScoped<EndwidthKServicer>();

                    Console.WriteLine("Server is running...");
                }).Build();

            host.Services.StartGrpcServer();
            await host.RunAsync();
        }
    }
}
