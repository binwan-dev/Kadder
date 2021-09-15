using System;
using System.Linq;
using System.Reflection;
using GenAssembly;
using Grpc.Core;
using Kadder;
using Kadder.Grpc.Server;
using Kadder.Grpc.Server.AspNetCore;
using Kadder.Utils;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class KadderGrpcServerServiceExtension
    {
        public static IServiceCollection UseGrpcServer(this IServiceCollection services, Action<GrpcServerBuilder> builderAction)
        {
            var builder = new GrpcServerBuilder();
            builderAction(builder);
            var server = new Server(builder.GrpcServerOptions.ChannelOptions);
            foreach (var port in builder.GrpcServerOptions.Ports)
                server.Ports.Add(port);

            var servicerTypes = ServicerHelper.GetServicerTypes(builder.Assemblies);
            var servicerProxyers = new ServicerProxyGenerator().Generate(servicerTypes);
            var namespaces = builder.GrpcServerOptions.PackageName;
            
            var codeBuilder = new CodeBuilder(namespaces, namespaces);
            codeBuilder.CreateClass(servicerProxyers.ToArray());
            codeBuilder.AddAssemblyRefence(Assembly.GetExecutingAssembly())
                .AddAssemblyRefence(typeof(ServerServiceDefinition).Assembly)
                .AddAssemblyRefence(typeof(ServiceProviderServiceExtensions).Assembly)
                .AddAssemblyRefence(typeof(Console).Assembly)
                .AddAssemblyRefence(servicerTypes.Select(p=>p.Assembly).Distinct().ToArray())
                .AddAssemblyRefence(typeof(KadderBuilder).Assembly)
                .AddAssemblyRefence(typeof(GrpcServerOptions).Assembly)
                .AddAssemblyRefence(builder.GetType().Assembly);
            
            var codeAssembly = codeBuilder.BuildAsync().Result;   
            foreach(var servicerProxyer in servicerProxyers)
            {
                namespaces = $"{servicerProxyer.Namespace}.{servicerProxyer.Name}";
                var proxyerType = codeAssembly.Assembly.GetType(namespaces);
                services.AddSingleton(proxyerType);
                builder.GrpcServicerProxyers.Add(proxyerType);
            }

            services.AddSingleton(server);
            services.AddSingleton(builder);
            services.AddSingleton(typeof(KadderBuilder), builder);
            
            return services;
        }
    }
}
