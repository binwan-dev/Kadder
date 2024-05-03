using Kadder.Utils.WebServer.Http;
using Kadder.Utils.WebServer.Http2;
using Kadder.Utils.WebServer.Socketing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;


var host = Host.CreateDefaultBuilder()
    .ConfigureServices((configuration, service) =>
    {
        service.AddLogging();

    })
    .Build();

var socketLog = host.Services.GetService<ILogger<Http2Server>>();
var connectionLog = host.Services.GetService<ILogger<TcpConnection>>();

var options = new HttpServerOptions()
{
    Address = "0.0.0.0",
    Port = 2335,
};
var frameHandler = new FrameHandler();
var connectionSetting = new ConnectionSetting();
var server = new Http2Server(options, socketLog,connectionSetting ,frameHandler);
server.Start();

Console.WriteLine("Hello, World!");
host.Run();
