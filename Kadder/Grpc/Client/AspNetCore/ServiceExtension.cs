using System;
using System.Linq;
using System.Reflection;
using GenAssembly;
using Grpc.Core;
using Kadder;
using Kadder.Grpc.Client;
using Kadder.Grpc.Client.AspNetCore;
using Kadder.Utils;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceExtension
    {
        public static IServiceCollection UseGrpcClient(this IServiceCollection services, Action<ClientBuilder> builderAction)
        {
            var builder = new ClientBuilder();
            builderAction(builder);

            var servicerTypes = ServicerHelper.GetServicerTypes(builder.Assemblies);
            var servicerProxyers = new ServicerProxyGenerator().Generate(servicerTypes);
            var namespaces = builder.GrpcServerOptions.PackageName;

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

            services.AddSingleton(builder);
            services.AddSingleton<IBinarySerializer, ProtobufBinarySerializer>();
            services.AddSingleton(typeof(KadderBuilder), builder);
            services.AddSingleton<ServicerInvoker>();
            services.AddSingleton<IObjectProvider, ObjectProvider>();

            var provider = services.BuildServiceProvider();
            KadderOptions.KadderObjectProvider = provider.GetService<IObjectProvider>();

            return services;

        }
    }
}
