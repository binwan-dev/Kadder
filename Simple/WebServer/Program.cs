using Kadder.Simple.WebServer;
using Kadder.WebServer.Socketing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var host = Host.CreateDefaultBuilder()
    .ConfigureServices((configuration, service) =>
    {
        service.AddLogging();

    })
    .Build();

var socketLog = host.Services.GetService<ILogger<ServerSocket>>();
var connectionLog = host.Services.GetService<ILogger<TcpConnection>>();

var serviceSocket = new ServerSocket("0.0.0.0", 2335, socketLog, connectionLog);
serviceSocket.Start();

long s = -1;
long.TryParse("2", out s);
Console.WriteLine(s);
// HttpClientTest.TestNoHeaderAndNoBody("127.0.0.1", 8080);

Console.WriteLine("Hello, World!");
host.Run();
