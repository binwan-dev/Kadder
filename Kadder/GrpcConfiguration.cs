using System;
using System.Threading.Tasks;
using GenAssembly;
using Kadder.Middlewares;
using Kadder.Utilies;
using Microsoft.Extensions.DependencyInjection;

namespace Kadder
{
    public static class GrpcConfiguration
    {
        public static IServiceCollection AddKadderGrpcClient(
            this IServiceCollection services, Action<GrpcClientBuilder> builderAction)
        {
            var builder = new GrpcClientBuilder();
            builderAction(builder);

            var serviceCallBuilder = new GrpcServiceCallBuilder();
            services.AddSingleton(builder);
            services.AddSingleton(serviceCallBuilder);
            services.RegSerializer(null, builder.BinarySerializer);

            foreach(var interceptor in builder.Interceptors)
            {
                services.AddSingleton(interceptor);
            }

            foreach (var clientMetadata in builder.ClientMetadatas)
            {
                var grpcClient = new GrpcClient(clientMetadata,builder, serviceCallBuilder);
                foreach(var interceptor in clientMetadata.PrivateInterceptors)
                {
                    services.AddSingleton(interceptor);
                }
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

        public static IServiceCollection AddKadderGrpcServer(
            this IServiceCollection services, Action<GrpcServerBuilder> builderAction)
        {
            var builder = new GrpcServerBuilder();
            builderAction(builder);
            var serviceBuilder = new GrpcServiceBuilder();
            services.AddSingleton(builder);
            services.RegSerializer(builder.JsonSerializer, builder.BinarySerializer);
            services.AddSingleton<GrpcHandlerBuilder>();
            services.AddSingleton(serviceBuilder);
            services.AddSingleton<GrpcMessageServicer>();
            foreach (var item in builder.Middlewares)
            {
                Middlewares.GrpcHandlerDirector.AddMiddleware(item);
            }
            foreach (var interceptor in builder.Interceptors)
            {
                services.AddSingleton(interceptor);
            }

            var namespaces = "Kadder.CodeGeneration";
            var codeBuilder = new CodeBuilder(namespaces, namespaces); 
            var grpcClasses = serviceBuilder.GenerateGrpcProxy(builder.Options, codeBuilder); 
            // var proxyCode = serviceBuilder.GenerateHandlerProxy(builder.Options.GetScanAssemblies(), codeBuilder);
            var codeAssembly = codeBuilder.BuildAsync().Result;
           
            foreach(var grpcClass in grpcClasses)
            {
                namespaces = $"{grpcClass.Namespace}.{grpcClass.Name}";
                var grpcType = codeAssembly.Assembly.GetType(namespaces);
                services.AddSingleton(grpcType);
                builder.Services.Add(grpcType);
            }
            services.AddSingleton<GrpcServer>();

            return services;
        }

        public static IServiceProvider StartKadderGrpc(this IServiceProvider provider)
        {   
            GrpcServerBuilder.ServiceProvider = provider;
            var server = provider.GetService<GrpcServer>();
            server.Start();
            return provider;
        }

        public static IServiceProvider ShutdownKadderGrpc(this IServiceProvider provider, Func<Task> action = null)
        {
            var server = provider.GetService<GrpcServer>();
            server.ShutdownAsync(action).Wait();
            return provider;
        }

        private static void RegSerializer(this IServiceCollection services, IJsonSerializer jsonSerializer,
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
