# Duplex C#模式

该模式下推荐使用nuget包的方式来引用，即创建 Server、Client、Protocol三个项目，将rpc服务以及接口对象放入Protocol项目中并打包成nuget包，提供给Server、Client使用。


## Protocol

1.  创建项目并引用Kadder
    
    ```sh
    dotnet new classlib
    dotnet add package Kadder
    ```
2.  声明Servicer以及对象
    
    ```csharp
    [ProtoContract]
    public class CreateNoteRequest
    {
        [ProtoMember(1)]
        public string Title{get;set;}
        [ProtoMember(2)]
        public string Content{get;set;}
    }
    
    [ProtoContract]
    public class CreateNoteResponse
    {
        [ProtoMember(1)]
        public string Title{get;set;}
        [ProtoMember(2)]
        public bool Status{get;set;}
    }
    
    [KServicer]
    public interface INoteServicer
    {
        Task<CreateNoteResponse> CreateAsync(CreateNoteRequest request);
    }
    ```


## Server

1.  创建项目
    
    ```sh
    dotnet new console && \
    dotnet add package Kadder &&\
    # 此处是上一步Protocol的nuget包
    dotnet add package Protocol &&\
    dotnet add Microsoft.Extensions.Hosting    
    ```

2.  实现Servicer
    
    ```csharp
    public class NoteServicer:INoteServicer
    {
        public Task<CreateNoteResponse> CreateAsync(CreateNoteRequest request)
        {
    	return Task.FromResult(new CreateNoteResponse()
    	{
    	    Title = request.Title,
    	    Status=true
    	});
        }
    }
    ```

3.  注册服务
    
    ```csharp
    var host = Host.CreateDefaultBuilder().UseGrpcServer((context,services,builder)=>
     {
        services.AddScoped<INoteServicer, NoteServicer>();
     })
     .Build();
    
    host.StartGrpcServer().Run();
    ```
    
    ```json
    // appsettings.json
    {
      "GrpcServer": {
        "Options": {
          "PackageName": "Note.Server",
          "Ports": [{ "Host": "0.0.0.0", "Port": 3002 }]
        },
        "AssemblyNames": ["Protocol"]
      }
    }
    ```


## Client

1.  创建项目
    
    ```sh
    dotnet new console
    dotnet add package Kadder
    dotnet add package Protocol
    dotnet add Microsoft.Extensions.Hosting
    ```

2.  注册并调用
    
    ```csharp
    var host = Host.CreateDefaultBuilder()
        .UseGrpcClient()
        .Build();
    
    var servicer = host.Services.GetService<INoteServicer>() ?? throw new ArgumentNullException(nameof(INoteServicer));
    var response = await servicer.CreateAsync(new CreateNoteRequest()
    {
        Title = "Note title",
        Content = "Note content"
    });
    ```
    
    ```json
    //appsettings.json
    {
        "GrpcClient": {
    	"ProxyerOptions": [
    	    {
    		"Name": "Note.Client",
    		"PackageName": "Note.Server",
    		"AssemblyNames": ["Protocol"],
    		"Addresses": [{"Address": "127.0.0.1:3002"}]
    	    }]
        }
    }
    ```