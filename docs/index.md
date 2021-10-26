# Kadder
## 关于
Kadder 是一个简单无代码侵入的RPC框架，目前暂只支持 GRPC 。Kadder 依赖于动态创建代码技术，可以让代码RPC调用变得简单高效且不损失性能。  
  
在服务端可以直接利用C#中的代码类来定义，无需编写Proto文件，降低GRPC门槛，甚至可以做到让你感受不到GRPC的存在。Kadder 建议采用 类库 Nuget包的方式来创建协议程序集，从而客户端只需要依赖协议程序集即可实现在客户端如同本地调用一般的方式来调用RPC。  
  
Talk is cheap, Let's write code simple!  
## 开始使用
### 创建服务端
1. 创建项目 `dotnet new console `  
2. 引用 Kadder 组件包 `dotnet add package Kadder`  
3. Program 启动类中加如下代码:  
```
static void main(string[] args)
{
    
}
```