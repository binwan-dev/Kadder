using System;
using System.Threading.Tasks;
using Kadder.Middlewares;
using Microsoft.Extensions.DependencyInjection;

namespace Kadder.Simple.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(Environment.CurrentDirectory);
            
            var services=new ServiceCollection();
            services.AddLogging();
            services.AddKadderGrpcServer(builder =>
            {
                builder.Options = new GrpcServerOptions()
                {
                    Host = "127.0.0.1",
                    Port = 3002,
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
            services.AddScoped<ImplServicer>();
            services.AddScoped<AttributeServicer>();
            services.AddScoped<EndwidthKServicer>();

            var provider = services.BuildServiceProvider();
            provider.StartKadderGrpc();

            Console.WriteLine("Server is running...");
            Console.ReadLine();
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
