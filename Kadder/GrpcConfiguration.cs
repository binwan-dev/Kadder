using System;
using System.Threading.Tasks;
using Atlantis.Common.CodeGeneration;
using Kadder.Middlewares;
using Kadder.Utilies;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Kadder
{
    public static class GrpcConfiguration
    {
        public static ServiceCollection UseKadderGrpcClient(
            this ServiceCollection services,
            Action<GrpcClientBuilder> builderAction)
        {
            var builder = new GrpcClientBuilder();
            builderAction(builder);
            services.AddSingleton<GrpcClientBuilder>();
            services.AddSingleton<GrpcServiceCallBuilder>();
            services.RegSerializer(null, builder.BinarySerializer);

            var provider = services.BuildServiceProvider();
            var serviceCallBuilder = provider.GetService<GrpcServiceCallBuilder>();
            var binarySerializer = provider.GetService<IBinarySerializer>();

            foreach (var options in builder.ClientOptions)
            {
                var grpcClient = new GrpcClient(options, serviceCallBuilder, binarySerializer);
                foreach (var item in grpcClient.GrpcServiceDic)
                {
                    services.AddSingleton(item.Key, item.Value);
                }
            }

            return services;
        }

        public static IServiceProvider ApplyKadderGrpcClient(this IServiceProvider provider)
        {
            GrpcClientBuilder.ServiceProvider = provider;
            return provider;
        }

        public static ServiceCollection UseKadderGrpcServer(
            this ServiceCollection services,
            Action<GrpcServerBuilder> builderAction)
        {
            var builder = new GrpcServerBuilder();
            builderAction(builder);
            services.AddSingleton<GrpcServerBuilder>(builder);
            services.RegSerializer(builder.JsonSerializer, builder.BinarySerializer);
            services.AddSingleton<GrpcHandlerBuilder>();
            services.AddSingleton<GrpcServiceBuilder>();
            services.AddSingleton<GrpcMessageServicer>();
            foreach (var item in builder.Middlewares)
            {
                Middlewares.GrpcHandlerDirector.AddMiddleware(item);
            }

            var provider = services.BuildServiceProvider();
            GrpcServerBuilder.ServiceProvider = provider;
            var serviceBuilder = provider.GetService<GrpcServiceBuilder>();

            var namespaces = "Kadder.CodeGeneration";
            var codeBuilder = new CodeBuilder(namespaces, namespaces);
            var grpcCode = serviceBuilder.GenerateGrpcProxy(
                builder.Options, codeBuilder);
            var proxyCode = serviceBuilder.GenerateHandlerProxy(
                builder.Options.GetScanAssemblies(), codeBuilder);
            var codeAssembly = codeBuilder.BuildAsync().Result;

            namespaces = $"{proxyCode.Namespace}.{proxyCode.Name}";
            var proxy = (IMessageServicerProxy)codeAssembly.Assembly
                .CreateInstance(namespaces);
            services.AddSingleton<IMessageServicerProxy>(proxy);

            namespaces = $"{grpcCode.Namespace}.{grpcCode.Name}";
            var grpcType = codeAssembly.Assembly.GetType(namespaces);
            services.AddSingleton(typeof(IGrpcServices), grpcType);
            services.AddSingleton<GrpcServer>();

            return services;
        }

        public static IServiceProvider StartKadderGrpc(
            this IServiceProvider provider)
        {   
            GrpcServerBuilder.ServiceProvider = provider;
            var server = provider.GetService<GrpcServer>();
            server.Start();
            return provider;
        }

        public static IServiceProvider ShutdownKadderGrpc(
            this IServiceProvider provider, Func<Task> action = null)
        {
            var server = provider.GetService<GrpcServer>();
            server.ShutdownAsync(action).Wait();
            return provider;
        }

        private static void RegSerializer(
            this IServiceCollection services,
            IJsonSerializer jsonSerializer,
            IBinarySerializer binarySerializer)
        {
            if (jsonSerializer == null)
            {
                services.AddSingleton<IJsonSerializer, NewtonsoftJsonSerializer>();
            }
            else
            {
                services.AddSingleton<IJsonSerializer>(jsonSerializer);
            }
            if (binarySerializer == null)
            {
                services.AddSingleton<IBinarySerializer, ProtobufBinarySerializer>();
            }
            else
            {
                services.AddSingleton<IBinarySerializer>(binarySerializer);
            }

        }
    }
}
