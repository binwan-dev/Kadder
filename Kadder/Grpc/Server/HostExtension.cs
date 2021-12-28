using System;
using System.Linq;
using System.Reflection;
using GenAssembly;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Kadder;
using Kadder.Grpc.Server;
using Kadder.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.Hosting
{
    public static class KadderGrpcServerHostExtension
    {
        public static IHostBuilder UseGrpcServer(this IHostBuilder hostBuilder, Action<HostBuilderContext, IServiceCollection, GrpcServerBuilder> builderAction = null, string configurationKeyName = "GrpcServer")
        {
            hostBuilder.ConfigureServices((context, services) =>
            {
                var log = services.BuildServiceProvider().GetService<ILogger<GrpcServerBuilder>>();

                var builder = context.Configuration.GetSection(configurationKeyName).Get<GrpcServerBuilder>() ?? new GrpcServerBuilder();
                builderAction?.Invoke(context, services, builder);

                var server = new Server(builder.Options.ChannelOptions);
                foreach (var port in builder.Options.Ports)
                    server.Ports.Add(new ServerPort(port.Host, port.Port, port.Credentials));
                foreach (var interceptor in builder.Interceptors)
                    services.AddSingleton(interceptor);

                foreach (var assemblyName in builder.AssemblyNames)
                    builder.Assemblies.Add(Assembly.Load(assemblyName));

		if(log.IsEnabled(LogLevel.Debug))
		    log.LogDebug(builder.ToString());

                var servicerTypes = ServicerHelper.GetServicerTypes(builder.Assemblies);
                if (servicerTypes == null || servicerTypes.Count == 0)
                    throw new ArgumentNullException("Not found any grpc servicer!");

                var servicerProxyers = new ServicerProxyGenerator(builder.Options.PackageName, servicerTypes).Generate();

                var codeBuilder = new CodeBuilder("Kadder.Grpc.Server");
                codeBuilder.CreateClass(servicerProxyers.ToArray());
                codeBuilder.AddAssemblyRefence(Assembly.GetExecutingAssembly())
		    .AddAssemblyRefence(typeof(ILogger).Assembly)
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
                    var namespaces = $"{servicerProxyer.Namespace}.{servicerProxyer.Name}";
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
                definition = definition.Intercept(intercetors);
                server.Services.Add(definition);
            }

            server.Start();

            return host;
        }
    }
}
