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
        public static IServiceCollection AddKadderClient(this IServiceCollection services, Action<GrpcClientBuilder> builderAction)
        {
            var builder = new GrpcClientBuilder();
            builderAction(builder);
            services.AddSingleton(builder);
            services.AddSingleton<IGrpcClientStrategy>(builder.Strategy);

            foreach (var interceptor in builder.Interceptors)
            {
                services.AddSingleton(interceptor);
            }

            foreach (var clientMetadata in builder.ClientMetadatas)
            {
                clientMetadata.PublicInterceptors = builder.Interceptors;

                var client = new GrpcClient(clientMetadata, builder);
                foreach (var interceptor in clientMetadata.PrivateInterceptors)
                {
                    services.AddSingleton(interceptor);
                }
                foreach (var callType in clientMetadata.GenerationCallTypes)
                {
                    services.AddSingleton(callType.Key, callType.Value);
                }
                GrpcClient.ClientDic.Add(clientMetadata.ID.ToString(), client);
            }

            return services;
        }

        public static IServiceProvider UseKadderClient(this IServiceProvider provider)
        {
            GrpcClientBuilder.ServiceProvider = provider;
            return provider;
        }

        public static IServiceCollection AddKadderServer(this IServiceCollection services, Action<GrpcServerBuilder> builderAction)
        {
            var serviceBuilder = new GrpcServiceBuilder();
            var builder = new GrpcServerBuilder();
            builderAction(builder);
            services.AddSingleton(builder);
            services.AddSingleton<GrpcHandlerBuilder>();
            services.AddSingleton(serviceBuilder);
            services.AddSingleton<GrpcMessageServicer>();
            services.AddSingleton<IBinarySerializer>(builder.BinarySerializer);
            foreach (var item in builder.Middlewares)
            {
                Middlewares.GrpcHandlerDirector.AddMiddleware(item);
            }
            foreach (var interceptor in builder.Interceptors)
            {
                services.AddSingleton(interceptor);
            }
            
            var codeBuilder = new CodeBuilder(builder.Options.NamespaceName, builder.Options.NamespaceName);
            var grpcClasses = serviceBuilder.GenerateGrpcProxy(builder, codeBuilder);
            var codeAssembly = codeBuilder.BuildAsync().Result;

            foreach (var grpcClass in grpcClasses)
            {
                var grpcType = codeAssembly.Assembly.GetType($"{grpcClass.Namespace}.{grpcClass.Name}");
                services.AddSingleton(grpcType);
                builder.Services.Add(grpcType);
            }
            services.AddSingleton<GrpcServer>();

            return services;
        }

        public static IServiceProvider StartKadderServer(this IServiceProvider provider)
        {
            GrpcServerBuilder.ServiceProvider = provider;
            var server = provider.GetService<GrpcServer>();
            server.Start();
            return provider;
        }

        public static IServiceProvider ShutdownKadderServer(this IServiceProvider provider, Func<Task> action = null)
        {
            var server = provider.GetService<GrpcServer>();
            server.ShutdownAsync(action).Wait();
            return provider;
        }

    }
}
