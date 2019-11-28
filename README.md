Kadder
=============================
[![](https://api.travis-ci.org/felixwan-git/Kadder.svg?branch=master)](https://www.travis-ci.org/felixwan-git/Kadder)

   Grpc框架的一种封装，提供了本地接口调用级，相比起原生实践要简单很多，这几乎是c#语言中grpc调用最简单的实践了。

   本组件不存在任何反射接口代码，其使用的是代码生成工具，不存在任何性能消耗，同时做到简单易用。

## 安装 Install
  ```
  dotnet add package Kadder
  ```

## 功能

- [x] 普通异步RPC请求调用
- [x] 中间件扩展
- [x] 兼容第三方序列化（Json、MessagePack等）
- [x] 支持.NetCore ServiceCollection 注册方式
- [x] 服务端支持生成Proto
- [ ] 客户端支持Proto调用
- [ ] 流式调用（Stream call）

## 使用 Use

1. 客户端和服务端都是c#情况：

   Server:   

```csharp  
            var services=new ServiceCollection();
            services.AddLogging();
            services.AddKadderGrpcServer(builder =>
            {
                builder.Options=new GrpcServerOptions();
            });
            services.AddScoped<IPersonMessageServicer, PersonMessageServicer>();
            
            var provider=services.BuildServiceProvider();
            provider.StartKadderGrpc();
    
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
```
    Client:
```csharp

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
```

