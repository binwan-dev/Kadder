using System;
using System.Threading.Tasks;
using Kadder.Middlewares;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Kadder.Simple.Server
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine(Environment.CurrentDirectory);

            var host=new Microsoft.Extensions.Hosting.HostBuilder()
                .ConfigureServices((context,services)=>{
                    services.AddLogging();
                    services.AddKadderGrpcServer(builder =>
                    {
                        builder.Options = new GrpcServerOptions()
                            {
                                Host = "0.0.0.0",
                                Port = 13002,
                                NamespaceName = "Atlantis.Simple",
                                ServiceName = "AtlantisService",
                                // ScanAssemblies = new string[]
                                // {
                                //     typeof(Program).Assembly.FullName
                                // }
                            };
                        Console.WriteLine(builder.Options.ScanAssemblies[0]);
                        builder.AddInterceptor<LoggerInterceptor>();
                    });
                    services.AddScoped<IPersonMessageServicer, PersonMessageServicer>();
                    services.AddScoped<IAnimalMessageServicer, AnimalMessageServicer>();
                    services.AddScoped<INumberMessageServicer, NumberMessageServicer>();
                    services.AddScoped<ImplServicer>();
                    services.AddScoped<AttributeServicer>();
                    services.AddScoped<EndwidthKServicer>();

                    Console.WriteLine("Server is running...");
                }).Build();

            host.Services.StartKadderGrpc();
            await host.RunAsync();
        }
    }

    public class TestMiddleware : Middlewares.GrpcMiddlewareBase
    {
        public TestMiddleware(HandlerDelegateAsync next) : base(next)
        {
        }

        protected override Task DoHandleAsync(GrpcContext context)
        {
            Console.WriteLine("heelo");
            return Task.CompletedTask;
        }
    }

}
