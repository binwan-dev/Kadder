using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var host = Host.CreateDefaultBuilder()
    .ConfigureLogging((context, builder) =>
    {
        builder.AddConsole();
        builder.AddDebug();
        builder.SetMinimumLevel(LogLevel.Debug);
    })
    .UseGrpcServer()
    .Build();

host.StartGrpcServer().Run();
