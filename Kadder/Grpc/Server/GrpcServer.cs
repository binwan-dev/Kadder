// using Grpc.Core;
// using System;
// using System.Collections.Generic;
// using System.Threading.Tasks;
// using Kadder.Middlewares;
// using Grpc.Core.Interceptors;

namespace Kadder
{
    public class GrpcServers
    {
        // private readonly Server _server;
        // private readonly GrpcServerOptions _options;
        // private readonly GrpcServerBuilder _builder;

        // public GrpcServer(GrpcServerBuilder builder)
        // {
        //     _builder = builder;
        //     _options = builder.Options ?? throw new ArgumentNullException("GrpcServerOption cannot be null");
        //     _server = new Server();
        //     _server.Ports.Add(new ServerPort(_options.Host, _options.Port, ServerCredentials.Insecure));
        // }

        // public GrpcServer Start()
        // {
        //     GrpcHandlerDirector.ConfigActor();
        //     foreach (var grpcServiceType in _builder.Services)
        //     {
        //         var grpcService = (IGrpcServices)GrpcServerBuilder.ServiceProvider.GetService(grpcServiceType);
        //         var definition = grpcService.BindServices();
        //         definition = ResolveInterceptors(definition);
        //         _server.Services.Add(definition);
        //     }
        //     _server.Start();
        //     return this;
        // }

        // private ServerServiceDefinition ResolveInterceptors(ServerServiceDefinition definition)
        // {
        //     if (_builder.Interceptors != null && _builder.Interceptors.Count > 0)
        //     {
        //         var interceptors = new List<Interceptor>();
        //         foreach (var interceptorType in _builder.Interceptors)
        //         {
        //             var interceptor = (Interceptor)GrpcServerBuilder.ServiceProvider.GetService(interceptorType);
        //             if (interceptor != null)
        //             {
        //                 interceptors.Add(interceptor);
        //             }
        //         }

        //         if (interceptors.Count > 0)
        //         {
        //             definition = definition.Intercept(interceptors.ToArray());
        //         }
        //     }
        //     return definition;
        // }

        // public async Task ShutdownAsync(Func<Task> action = null)
        // {
        //     await _server.ShutdownAsync();
        //     if (action != null)
        //     {
        //         await action.Invoke();
        //     }
        // }
    }

}
