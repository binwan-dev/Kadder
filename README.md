Kadder
=============================
[![](https://api.travis-ci.org/felixwan-git/Kadder.svg?branch=master)](https://www.travis-ci.org/felixwan-git/Kadder)

   Grpc框架的一个封装框架，提供了本地接口调用级，相比起原生实践要简单很多，这几乎是c#语言中grpc调用最简单的实践了。

   本框架不存在任何反射接口代码，其使用的是代码生成工具，不存在任何性能消耗，同时做到简单易用。

## 安装 Install
  ```
  dotnet add package Kadder
  ```

## 使用 Use
1. 客户端和服务端都是c#情况：

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
            
            var services=new ServiceCollection();
            services.AddLogging();
            services.AddKadderGrpcServer(builder =>
            {
                builder.Options=new GrpcServerOptions();
            });
            services.AddScoped<IPersonMessageServicer, PersonMessageServicer>();
            
            var provider=services.BuildServiceProvider();
            provider.StartKadderGrpc();

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
            ...
        }
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
           var options=new GrpcOptions();

            IServiceCollection services=new ServiceCollection();
            services.AddLogging();
            services.AddKadderGrpcClient(builder=>
            {
                builder.RegClient(options);
            });
            
            var provider=services.BuildServiceProvider();
            provider.ApplyKadderGrpcClient();
            
            var servicer=provider.GetService<IPersonMessageServicer>();
            var message=new HelloMessage(){Name="DotNet"};
            var result=servicer.HelloAsync(message).Result;
            Console.WriteLine(result.Result);
        }
    }
}
```

