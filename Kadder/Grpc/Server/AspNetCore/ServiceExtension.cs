using System;
using GenAssembly;
using Grpc.Core;
using Kadder;
using Kadder.GrpcServer;
using Kadder.Utils;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class KadderGrpcServerServiceExtension
    {
        public static IServiceCollection UseGrpcServer(this IServiceCollection services, Action<GrpcServerBuilder> builderAction)
        {
            var builder = new GrpcServerBuilder();
            builderAction(builder);
            var server = new Server(builder.GrpcServerOptions.ChannelOptions);
            foreach (var port in builder.GrpcServerOptions.Ports)
                server.Ports.Add(port);

            var servicerTypes = ServicerHelper.GetServicerTypes(builder.Assemblies);

            var namespaces = builder.GrpcServerOptions.PackageName;
            var codeBuilder = new CodeBuilder(namespaces, namespaces);
            var grpcClasses = serviceBuilder.GenerateGrpcProxy(builder.Options, codeBuilder);
            // var proxyCode = serviceBuilder.GenerateHandlerProxy(builder.Options.GetScanAssemblies(), codeBuilder);
            var codeAssembly = codeBuilder.BuildAsync().Result;


            services.AddSingleton(server);
            services.AddSingleton(builder);
            services.AddSingleton(typeof(KadderBuilder), builder);


            return builder;
        }
    }
}
