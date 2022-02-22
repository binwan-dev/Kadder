using Kadder.Simple.Duplex.Protocol;
using Kadder.Simple.Duplex.Protocol.Protocols;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var host = Host.CreateDefaultBuilder()
    .ConfigureLogging((context, builder) =>
    {
        builder.AddConsole();
        builder.SetMinimumLevel(LogLevel.Debug);
    })
    .UseGrpcClient()
    .Build();

var servicer = host.Services.GetService<INoteServicer>() ?? throw new ArgumentNullException(nameof(INoteServicer));
var response = await servicer.CreateAsync(new CreateNoteRequest()
{
    Title = "Note title",
    Content = "Note content"
});
// See https://aka.ms/new-console-template for more information
var status = response.Status ? "success" : "failed";
Console.WriteLine($"Create note {status}, title: {response.Title}");
