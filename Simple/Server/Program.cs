using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Reflection;
using Kadder.Utils;

var assemblies = new List<Assembly>();
assemblies.Add(Assembly.LoadFile("/Users/binwan/Documents/binwan-dev/Kadder/Simple/Server/temp/Server.dll"));
var servicerTypes=ServicerHelper.GetServicerTypes(assemblies);
foreach(var s in servicerTypes)
    Console.WriteLine(s.FullName);

var host = Host.CreateDefaultBuilder()
    .ConfigureLogging((context,builder)=>
    {
	builder.SetMinimumLevel(LogLevel.Debug);
	builder.AddConsole();
	builder.Services.AddLogging();
    })
    .UseGrpcServer()
    .Build();

host.StartGrpcServer().Run();

Console.WriteLine("Hello, World!");
