## 仅编写服务端
1. 创建项目 `dotnet new console`
2. 添加Kadder `dotnet add package Kadder`
3. 创建并声明服务类.  
```csharp
public record CreateMessage(string Title,string Content);
public record CreateMessageResult(bool Status);

public class NoteServicer:IMessagingServicer
{
    public Task<CreateMessageResult> CreateAsync(CreateMessage request)
    {
        return Task.FromResult(new CreateMessageResult(true));
    }
}
```
4. 注册NoteServicer服务并启动GrpcServer
```csharp  
IHostBuilder builder = Host.CreateDefaultBuilder();
builder.UseGrpcServer();

IHost host = builder.Build();
host.StartGrpcServer();
```
```json
// appsettings.json
{
  "GrpcServer": {
    "Options": {
      "PackageName": "Kadder.NoteServer",
      "Ports": [{ "Host": "0.0.0.0", "Port": 3002 }]
    }
  }
```  
### 生成Proto文件
1. 安装 proto2csharp 工具 `dotnet tool install --global proto2csharp`
2. 运行命令 
```bash
proto2csharp proto -i ./NoteServer/ -o ./notesrv.proto -p notesrv
# proto2csharp <proto|project> -i <csproj dir> -o <proto output file> -p <package>
```