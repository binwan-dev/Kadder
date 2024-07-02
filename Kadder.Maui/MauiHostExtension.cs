using Kadder.Grpc.Client;

namespace Kadder.Maui;

public static class MauiHostExtension
{
    public static MauiAppBuilder AddGrpcClient(this MauiAppBuilder hostBuilder,
        Action<MauiAppBuilder, ClientBuilder>? builderAction = null,
        string configurationKeyName = "GrpcClient")
    {
        hostBuilder.Services.AddGrpcClient(hostBuilder.Configuration, (clientBuilder, configuration, services) =>
        {
            builderAction?.Invoke(hostBuilder, clientBuilder);
        });
        return hostBuilder;
    }

    public static MauiApp UseGrpcClient(this MauiApp app)
    {
        app.Services.UseGrpcClient();
        return app;
    }
}