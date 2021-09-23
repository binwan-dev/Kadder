using System;
using Microsoft.Extensions.DependencyInjection;

namespace Kadder.Utils
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
