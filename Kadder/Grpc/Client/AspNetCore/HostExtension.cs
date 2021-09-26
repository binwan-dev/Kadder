using System;
using System.Linq;
using System.Reflection;
using GenAssembly;
using Grpc.Core;
using Kadder;
using Kadder.Grpc.Client;
using Kadder.Grpc.Client.AspNetCore;
using Kadder.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.Hosting
{
    public static class KadderGrpcClientHostExtension
    {
        public static IHostBuilder UseGrpcClient(this IHostBuilder hostBuilder, Action<HostBuilderContext, IServiceCollection, ClientBuilder> builderAction = null, string configurationKeyName = "GrpcClient")
        {
            hostBuilder.ConfigureServices((context, services) =>
            {
                var builder = context.Configuration.GetSection(configurationKeyName).Get<ClientBuilder>() ?? new ClientBuilder();
                builderAction?.Invoke(context, services, builder);

                var codeBuilder = new CodeBuilder("CodeGenerate");
                foreach (var client in builder.Clients)
                {
                    codeBuilder.CreateClass(new ServicerProxyGenerator(client.Options.PackageName, client.ServicerTypes.ToList()).Generate().ToArray());
                    codeBuilder.AddAssemblyRefence(client.Options.Assemblies.ToArray());
                }
                codeBuilder.AddAssemblyRefence(Assembly.GetExecutingAssembly())
                    .AddAssemblyRefence(typeof(ServerServiceDefinition).Assembly)
                    .AddAssemblyRefence(typeof(ServiceProviderServiceExtensions).Assembly)
                    .AddAssemblyRefence(typeof(Console).Assembly)
                    .AddAssemblyRefence(typeof(KadderBuilder).Assembly)
                    .AddAssemblyRefence(typeof(GrpcServerOptions).Assembly)
                    .AddAssemblyRefence(builder.GetType().Assembly);

                var codeAssembly = codeBuilder.BuildAsync().Result;
                foreach (var servicerProxyer in codeBuilder.Classes)
                {
                    var namespaces = $"{servicerProxyer.Namespace}.{servicerProxyer.Name}";
                    var proxyerType = codeAssembly.Assembly.GetType(namespaces);
                    var servicerType = proxyerType.BaseType;
                    if (servicerType == typeof(object))
                        servicerType = builder.ServicerTypes.FirstOrDefault(p => p.FullName == proxyerType.GetInterfaces()[0].FullName);
                    services.AddSingleton(servicerType, proxyerType);
                }

                services.AddSingleton(builder);
                services.AddSingleton<IBinarySerializer, ProtobufBinarySerializer>();
                services.AddSingleton(typeof(KadderBuilder), builder);
                services.AddSingleton<ServicerInvoker>();
                services.AddSingleton<IObjectProvider, ObjectProvider>();

                var provider = services.BuildServiceProvider();
                KadderOptions.KadderObjectProvider = provider.GetService<IObjectProvider>();

            });
            return hostBuilder;
        }
    }
}