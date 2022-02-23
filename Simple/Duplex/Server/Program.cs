using Kadder.Simple.Duplex.Protocol;
using Kadder.Simple.Duplex.Server;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var host = Host.CreateDefaultBuilder()
    .ConfigureLogging((context, builder) =>
    {
        builder.AddConsole();
        builder.AddDebug();
        builder.SetMinimumLevel(LogLevel.Debug);
    })
    .UseGrpcServer((context,services,builder)=>
    {
        services.AddScoped<INoteServicer, NoteServicer>();
    })
    .Build();

host.StartGrpcServer().Run();
