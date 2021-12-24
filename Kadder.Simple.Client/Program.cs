using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Kadder.Grpc.Client.Options;
using Grpc.Core;
using Microsoft.Extensions.Hosting;
using Kadder.Simple.Protocol.Servicers;
using Kadder.Simple.Protocol.Requests;

namespace Atlantis.Grpc.Simple.Client
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var host = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder()
                .UseEnvironment("Development")
                .UseGrpcClient((context, servicers, builder) =>
                {
                    var proxyerOptions = new GrpcProxyerOptions();
                    proxyerOptions.PackageName = "Kadder.Servicer";
                    proxyerOptions.Addresses.Add(new GrpcChannelOptions() { Address = "127.0.0.1:3002", Credentials = ChannelCredentials.Insecure });
                    proxyerOptions.AddAssembly(typeof(IPersonServicer).Assembly);

                    builder.AddProxyer(proxyerOptions);
                })
                .Build();

            var provider = host.Services;
            var personServicer = provider.GetService<IPersonServicer>();
            var response = await personServicer.HelloAsync(new HelloRequest() { Msg = "Hello, I'm Client!" });
            Console.WriteLine(response.Msg);
        }
    }
}
