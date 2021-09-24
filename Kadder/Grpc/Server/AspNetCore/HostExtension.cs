using System;
using System.Linq;
using System.Reflection;
using GenAssembly;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Kadder;
using Kadder.Grpc.Server;
using Kadder.Grpc.Server.AspNetCore;
using Kadder.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Microsoft.Extensions.Hosting
{
    public static class KadderGrpcServerHostExtension
    {
        public static IHostBuilder UseGrpcServer(this IHostBuilder hostBuilder, Action<HostBuilderContext, IServiceCollection, GrpcServerBuilder> builderAction = null, string configurationKeyName = "GrpcServer")
        {
            hostBuilder.ConfigureServices((context, services) =>
            {
                var builder = context.Configuration.GetSection(configurationKeyName).Get<GrpcServerBuilder>() ?? new GrpcServerBuilder();
                builderAction?.Invoke(context, services, builder);

                var server = new Server(builder.Options.ChannelOptions);
                foreach (var port in builder.Options.Ports)
                    server.Ports.Add(new ServerPort(port.Host, port.Port, port.Credentials));
                foreach (var interceptor in builder.Interceptors)
                    services.AddSingleton(interceptor);

                var servicerTypes = ServicerHelper.GetServicerTypes(builder.Assemblies);
                var servicerProxyers = new ServicerProxyGenerator().Generate(servicerTypes);
                var namespaces = builder.Options.PackageName;

                var codeBuilder = new CodeBuilder(namespaces, namespaces);
                codeBuilder.CreateClass(servicerProxyers.ToArray());
                codeBuilder.AddAssemblyRefence(Assembly.GetExecutingAssembly())
                    .AddAssemblyRefence(typeof(ServerServiceDefinition).Assembly)
                    .AddAssemblyRefence(typeof(ServiceProviderServiceExtensions).Assembly)
                    .AddAssemblyRefence(typeof(Console).Assembly)
                    .AddAssemblyRefence(servicerTypes.Select(p => p.Assembly).Distinct().ToArray())
                    .AddAssemblyRefence(typeof(KadderBuilder).Assembly)
                    .AddAssemblyRefence(typeof(GrpcServerOptions).Assembly)
                    .AddAssemblyRefence(builder.GetType().Assembly);

                var codeAssembly = codeBuilder.BuildAsync().Result;
                foreach (var servicerProxyer in servicerProxyers)
                {
                    namespaces = $"{servicerProxyer.Namespace}.{servicerProxyer.Name}";
                    var proxyerType = codeAssembly.Assembly.GetType(namespaces);
                    services.AddSingleton(proxyerType);
                    builder.GrpcServicerProxyers.Add(proxyerType);
                }

                services.AddSingleton(server);
                services.AddSingleton(builder);
                services.AddSingleton<IBinarySerializer, ProtobufBinarySerializer>();
                services.AddSingleton(typeof(KadderBuilder), builder);
                services.AddSingleton<IObjectProvider, ObjectProvider>();
            });
            return hostBuilder;
        }

        public static IHost StartGrpcServer(this IHost host)
        {
            var provider = host.Services;
            var builder = provider.GetService<GrpcServerBuilder>();
            var server = provider.GetService<Server>();

            var intercetors = new Interceptor[builder.Interceptors.Count];
            for (var i = 0; i < builder.Interceptors.Count; i++)
                intercetors[i] = (Interceptor)provider.GetService(builder.Interceptors[i]);

            foreach (var serviceProxyer in builder.GrpcServicerProxyers)
            {
                var definition = ((IGrpcServices)provider.GetService(serviceProxyer)).BindServices();
                definition.Intercept(intercetors);
                server.Services.Add(definition);
            }

            server.Start();

            return host;
        }
    }
}