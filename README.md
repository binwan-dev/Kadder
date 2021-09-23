Kadder
=============================
[![](https://api.travis-ci.org/felixwan-git/Kadder.svg?branch=master)](https://www.travis-ci.org/felixwan-git/Kadder)

   Kadder 致力于简化C# RPC 调用过程，目前支持 Grpc 协议。Kadder使用事先约定模式开发。  
   
###Grpc模式  
   该模式下使用 Namespace 代替 Proto 中 Package，使用接口和类代理Proto接口和消息体，从而无需拷贝、书写Proto文件，以C#接口和结构体来书写。  
   以下即可声明一个Proto协议，Grpc接口。  
   对了，对比原始GRpc实现，Kadder可以支持 无参请求 和 无参返回。
```
namespace Kadder
{
    public interface IPersonServicer:IMessageServicer
    {
        Task<Result> HelloAsync(Request request); 

        Task HelloVoidAsync();// 可以支持无参；
    }
}
```
## 安装 Install
  ```
  dotnet add package Kadder
  ```

## 使用 Use

请查看 Simple 案例 或查看说明。