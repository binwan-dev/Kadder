using System;
using System.Linq;
using System.Reflection;
using GenAssembly;
using Grpc.Core;
using Kadder.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Kadder.Grpc.Client;

public static class ServiceExtension
{
    public static IServiceCollection AddGrpcClient(this IServiceCollection services,
        IConfiguration configuration,
        Action<ClientBuilder,IConfiguration,IServiceCollection> clientBuilderAction=null,
        string configurationKeyName="GrpcClient")
    {
        var builder = configuration.GetSection(configurationKeyName).Get<ClientBuilder>() ?? new ClientBuilder();
        clientBuilderAction?.Invoke(builder, configuration, services);
        var client = builder.Build();

        var codeBuilder = new CodeBuilder("CodeGenerate");
        if (!string.IsNullOrWhiteSpace(builder.CodeCacheDir))
            CodeBuilder.CodeCachePath = builder.CodeCacheDir;
        if (!string.IsNullOrWhiteSpace(builder.DllCacheDir))
            CodeBuilder.DllCachePath = builder.DllCacheDir;
        
        foreach (var proxyer in client.Proxyers)
        {
            codeBuilder.CreateClass(
                new ServicerProxyGenerator(proxyer.Options.PackageName, proxyer.ServicerTypes.ToList()).Generate()
                    .ToArray());
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
            var proxyerType = codeAssembly.Assembly.GetType(namespaces) ?? throw new InvalidOperationException("Cannot get CodeAssembly");
            var servicerType = proxyerType?.GetInterfaces()?.FirstOrDefault();
            if (servicerType == null)
                services.AddSingleton(proxyerType!);
            else
                services.AddSingleton(servicerType, proxyerType!);
        }

        foreach (var proxyerOptions in client.ProxyerOptions)
        foreach (var interceptor in proxyerOptions.Interceptors)
            services.AddSingleton(interceptor);

        services.AddSingleton(client);
        services.AddSingleton<IBinarySerializer, ProtobufBinarySerializer>();
        services.AddSingleton<ServicerInvoker>();
        services.AddSingleton<IObjectProvider, ObjectProvider>();
        return services;
    }

    public static IServiceProvider UseGrpcClient(this IServiceProvider serviceProvider)
    {
        KadderOptions.KadderObjectProvider = serviceProvider.GetService<IObjectProvider>();
        return serviceProvider;
    }
}