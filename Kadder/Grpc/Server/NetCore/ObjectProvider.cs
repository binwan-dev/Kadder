using System;
using Kadder.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace Kadder.Grpc.Server.NetCore
{
    public class ObjectProvider : IObjectProvider
    {
        private readonly IServiceProvider _serviceProvider;

        public ObjectProvider(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

        public IObjectScope CreateScope() => new ObjectScope(_serviceProvider.CreateScope());

        public T GetObject<T>() => _serviceProvider.GetService<T>();

        public object GetObject(Type type) => _serviceProvider.GetService(type);
    }
}