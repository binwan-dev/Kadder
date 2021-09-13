using Kadder.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace Kadder.Grpc.Server.NetCore
{
    public class ObjectScope : IObjectScope
    {
        private readonly IServiceScope _scope;
        private readonly IObjectProvider _provider;

        public ObjectScope(IServiceScope scope)
        {
            _scope = scope;
            _provider = new ObjectProvider(scope.ServiceProvider);
        }

        public IObjectProvider Provider => _provider;

        public void Dispose() => _scope.Dispose();
    }
}
