/*
 * @Description: Registr grpc client for netcore project
 * @Author: Bin Wan
 * @Email: email@wanbin.tech
 */
using System;
using System.Linq;
using System.Reflection;
using GenAssembly;
using Grpc.Core;
using Kadder;
using Kadder.Grpc.Client;
using Kadder.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.Hosting
{
    public static class KadderGrpcClientHostExtension
    {
        /// <summary>
        /// Register GrpcClient
        /// </summary>
        /// <param name="hostBuilder">The host builder context</param>
        /// <param name="builderAction">The client options builder action</param>
        /// <param name="configurationKeyName">The client optios key in appsetting.json</param>
        /// <returns></returns>
        public static IHostBuilder UseGrpcClient(this IHostBuilder hostBuilder, Action<HostBuilderContext, IServiceCollection, ClientBuilder> builderAction = null, string configurationKeyName = "GrpcClient")
        {
            hostBuilder.ConfigureServices((context, services) =>
            {
                var builder = context.Configuration.GetSection(configurationKeyName).Get<ClientBuilder>() ?? new ClientBuilder();
                builder.Configuration = context.Configuration;
                builder.Services = services;
                builderAction?.Invoke(context, services, builder);
                var client = builder.Build();

                var codeBuilder = new CodeBuilder("CodeGenerate");
                foreach (var proxyer in client.Proxyers)
                {
                    codeBuilder.CreateClass(new ServicerProxyGenerator(proxyer.Options.PackageName, client.ServicerTypes.ToList()).Generate().ToArray());
                    codeBuilder.AddAssemblyRefence(proxyer.Options.Assemblies.ToArray());
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
                        servicerType = client.ServicerTypes.FirstOrDefault(p => p.FullName == proxyerType.GetInterfaces()[0].FullName);
                    services.AddSingleton(servicerType, proxyerType);
                }

                foreach (var proxyerOptions in client.ProxyerOptions)
                    foreach (var interceptor in proxyerOptions.Interceptors)
                        services.AddSingleton(interceptor);

                services.AddSingleton(client);
                services.AddSingleton<IBinarySerializer, ProtobufBinarySerializer>();
                services.AddSingleton<ServicerInvoker>();
                services.AddSingleton<IObjectProvider, ObjectProvider>();

                var provider = services.BuildServiceProvider();
                KadderOptions.KadderObjectProvider = provider.GetService<IObjectProvider>();

            });
            return hostBuilder;
        }
    }
}
