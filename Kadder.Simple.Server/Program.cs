using System;
using System.Threading.Tasks;
using Kadder.Middlewares;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Kadder.Simple.Server
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine(Environment.CurrentDirectory);
            var ip = "127.0.0.1";
            var port = 3002;
            if (args.Length > 0)
            {
                var arr = args[0].Split(':');
                ip = arr[0];
                port = int.Parse(arr[1]);
            }

            var host = new Microsoft.Extensions.Hosting.HostBuilder()
                .ConfigureLogging(builder=>
                {
                    builder.AddConsole();
                })
                .ConfigureServices((context, services) =>
                {
                    services.AddLogging();
                    services.AddKadderServer(builder =>
                    {
                        builder.Options = new GrpcServerOptions()
                        {
                            Host = ip,
                            Port = port,
                            NamespaceName = "Atlantis.Simple",
                            ServiceName = "AtlantisService",
                            IsGeneralProtoFile=false
                            // ScanAssemblies = new string[]
                            // {
                            //     typeof(Program).Assembly.FullName
                            // }
                        };
                        Console.WriteLine(builder.Options.ScanAssemblies[0]);
                        builder.AddInterceptor<LoggerInterceptor>();
                        builder.UseTextJsonSerializer();
                    });
                    services.AddScoped<IPersonMessageServicer, PersonMessageServicer>();
                    services.AddScoped<IAnimalMessageServicer, AnimalMessageServicer>();
                    services.AddScoped<INumberMessageServicer, NumberMessageServicer>();
                    services.AddScoped<JsonMessageKServicer>();
                    services.AddScoped<ImplServicer>();
                    services.AddScoped<AttributeServicer>();
                    services.AddScoped<EndwidthKServicer>();

                    Console.WriteLine("Server is running...");
                }).Build();

            host.Services.StartKadderServer();
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
