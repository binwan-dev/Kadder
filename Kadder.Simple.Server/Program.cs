using System;
using System.Threading.Tasks;
using Kadder.Middlewares;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Kadder;

namespace Kadder.Simple.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(Environment.CurrentDirectory);
            var options=new GrpcServerOptions()
            {
                Host="127.0.0.1",
                Port=3002,
                NamespaceName="Atlantis.Simple",
                PackageName="Atlantis.Simple",
                ServiceName="AtlantisService",
                ScanAssemblies=new string[]
                {
                    typeof(Program).Assembly.FullName
                }
            };
            //GrpcConfiguration.LoggerFactory=new Loggerfac();

            IServiceCollection services=new ServiceCollection();
            services.AddLogging();
            services.AddKadderGrpcServer(builder=>
            {
                 builder.Options= options;
                 builder.AddMiddleware<TestMiddleware>();
            });
            services.AddScoped<IPersonMessageServicer, PersonMessageServicer>();

            var provider=services.BuildServiceProvider();

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
