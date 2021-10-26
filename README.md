Kadder
=============================
[![](https://api.travis-ci.org/felixwan-git/Kadder.svg?branch=master)](https://www.travis-ci.org/felixwan-git/Kadder)

   Kadder 致力于简化C# RPC 调用过程，目前支持 Grpc 协议。Kadder使用事先约定模式开发。  
   
### Grpc模式   
   该模式下使用 Namespace 代替 Proto 中 Package，使用接口和类代理Proto接口和消息体，从而无需拷贝、书写Proto文件，以C#接口和结构体来书写。  
   以下即可声明一个Proto协议，Grpc接口。  
```
namespace Kadder
{
    public interface IPersonServicer:IMessageServicer
    {
        // 普通模式
        Task<Result> HelloAsync(Request request); 

        // 无参普通模式
        Task HelloVoidAsync();// 可以支持无参；
        
        // 客户端流模式
        Task<Result> HelloClientStreamAsync(IAsyncRequestStream<Request> request);
        
        // 服务端流模式
        Task HelloServerStreamAsync(Request request, IAsyncResponseStream<HelloMessageResult> response);
        
        // 双向流模式
        Task HelloDuplexStreamAsync(IAsyncRequestStream<HelloMessage> request, IAsyncResponseStream<HelloMessageResult> response);
    }
}
```  
## 客户端调用
```
namespace Client
{
   public class Program
   {
      public static void main(string[] args)
      {
         // 直接通过 ioc 组件拿到service，直接调用即可。
         var service = serviceProvider.GetService<IPersonServicer>();
         service.HelloAsync(request);
      }
   }
}
```
## 安装 Install
  ```
  dotnet add package Kadder
  ```

## 使用 Use

请查看 Simple 案例 或查看说明。
