/*
 * @Description: Registr grpc client for netcore project
 * @Author: Bin Wan
 * @Email: email@wanbin.tech
 */
using System;
using Kadder.Grpc.Client;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.Hosting;

public static class KadderGrpcClientHostExtension
{
    /// <summary>
    /// Register GrpcClient
    /// </summary>
    /// <param name="hostBuilder">The host builder context</param>
    /// <param name="builderAction">The client options builder action</param>
    /// <param name="configurationKeyName">The client optios key in appsetting.json</param>
    /// <returns></returns>
    public static IHostBuilder AddGrpcClient(this IHostBuilder hostBuilder,
        Action<HostBuilderContext, IServiceCollection, ClientBuilder> builderAction = null,
        string configurationKeyName = "GrpcClient")
    {
        hostBuilder.ConfigureServices((context, services) =>
        {
            services.AddGrpcClient(context.Configuration, (clientBuilder, configuration, services) =>
            {
                builderAction?.Invoke(context, services, clientBuilder);
            });
        });
        return hostBuilder;
    }

    public static IHost UseGrpcClient(this IHost host)
    {
        host.Services.UseGrpcClient();
        return host;
    }
}