# 仅编写服务端

1.  创建项目
    
    ```sh
    dotnet new console
    ```
2.  添加Kadder
    
    ```sh
    dotnet add package Kadder
    ```

3.  创建并声明服务类.
    
    ```csharp
    public record CreateMessage(string Title,string Content);
    public record CreateMessageResult(bool Status);
    
    [KServicer]
    public class NoteServicer
    {
    	public Task<CreateMessageResult> CreateAsync(CreateMessage request)
    	{
    		return Task.FromResult(new CreateMessageResult(true));
    	}
    }
    ```

4.  配置appsettings.json
    
    ```js
    // appsettings.json
    {
    	"GrpcServer": {
    		"Options": {
    			"PackageName": "Kadder.NoteServer",
    			"Ports": [{ "Host": "0.0.0.0", "Port": 3002 }]
    		}
    	}
    }
    
    ```

5.  注册NoteServicer服务并启动GrpcServer
    
    ```csharp
    Console.WriteLine("dd");
    
    IHostBuilder builder = Host.CreateDefaultBuilder();
    builder.UseGrpcServer();
    
    IHost host = builder.Build();
    host.StartGrpcServer();
    ```


# 生成Proto文件

1.  安装 proto2csharp 工具
    
    ```sh
    dotnet tool install --global proto2csharp
    ```

2.  运行命令
    
    ```sh
    proto2csharp proto -i ./NoteServer/ -o ./notesrv.proto -p notesrv
    # proto2csharp <proto|project> -i <csproj dir> -o <proto output file> -p <package>
    ```