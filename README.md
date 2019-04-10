Atlantis.Grpc
=============================
  该组件解决Grpc在C#下的一个封装,依赖于Grpc.Core.使用代码生成dll的方式对GRPC进行包装,与传统的中间件相比,代码入侵度,性能,易用性都有提高.组件实现了
Server与Client端的实现,Client仅适用于Server和Client都是C#环境. 
  组件默认带有IOC接口以及Logger和Json等组件,如需自定义集成,请在GrpcOption中传入相应的实现即可. 
  This component solves the encapsulation of Grpc under C# and relies on Grpc. Core. It uses code generation DLL to package GRPC. Compared with traditional middleware, the code intrusion, performance and usability are improved. Component implementation.
For the implementation of Server and C lient, C lient is only applicable to both Server and C lient in C_environment. 
  Components default with IOC interface and Logger and Json and other components, if you need to customize integration, please pass in the corresponding implementation in GrpcOption.

## 安装 Install
  ```
  dotnet add package Atlantis.Grpc
  ```
  
## 使用 Use
  1. 服务端和客户端都是C# Server&Client use language with c#:  
    Server:  
```csharp  
using System;
using System.Reflection;
using Atlantis.Grpc.Utilies;

namespace Atlantis.Grpc.Simple.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(Environment.CurrentDirectory);
            var options=new GrpcServerOptions()
            {
                Host="127.0.0.1",
                Port=3002,
                NamespaceName="Atlantis.Simple",
                PackageName="Atlantis.Simple",
                ServiceName="AtlantisService",
                ScanAssemblies=new Assembly[]
                {
                    typeof(Program).Assembly
                }
            };

            var server=new GrpcServer(options);
            ObjectContainer.Register<IPersonMessageServicer,PersonMessageServicer>(LifeScope.Single);
            server.Start();

            Console.WriteLine("Server is running...");
            Console.ReadLine();
        }
    }
    
    public interface IPersonMessageServicer:IMessagingServicer
    {
        Task<HelloMessageResult> HelloAsync(HelloMessage message);
    }

    public class PersonMessageServicer : IPersonMessageServicer
    {
        public Task<HelloMessageResult> HelloAsync(HelloMessage message)
        {
            var result = $"Hello, {message.Name}";
            return Task.FromResult(new HelloMessageResult()
            {
                Result = result
            });
        }
    }

    [ProtoContract(ImplicitFields=ImplicitFields.AllPublic)]
    public class HelloMessage : BaseMessage
    {
        public string Name { get; set; }
    }

    [ProtoContract(ImplicitFields=ImplicitFields.AllPublic)]
    public class HelloMessageResult : GrpcMessageResult
    {
        public string Result { get; set; }
    }
}
```
    Client:
```csharp
using System;
using System.Reflection;
using Atlantis.Grpc.Simple.Server;
using Atlantis.Grpc.Utilies;
// using Atlantis.Simple;
using Grpc.Core;
// using static Atlantis.Simple.AtlantisService;

namespace Atlantis.Grpc.Simple.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            var options=new GrpcOptions()
            {
                Host="127.0.0.1",
                Port=3002,
                NamespaceName="Atlantis.Simple",
                PackageName="Atlantis.Simple",
                ServiceName="AtlantisService",
                ScanAssemblies=new Assembly[]
                {
                    typeof(IPersonMessageServicer).Assembly
                }
            };
            var client=new GrpcClient(options);
            var servicer=client.GetService<IPersonMessageServicer>();
            var message=new HelloMessage(){Name="DotNet"};
            var result=servicer.HelloAsync(message).Result;
            Console.WriteLine(result.Result);
        }
    }
}
```
