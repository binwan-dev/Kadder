using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Kadder.Middlewares;
using Microsoft.Extensions.Options;
using Grpc.Core.Interceptors;
using System.Linq;

namespace Kadder
{
    public class GrpcServer
    {
        private readonly Server _server;
        private readonly GrpcServerOptions _options;
        private readonly GrpcServerBuilder _builder;
        private readonly IGrpcServices _grpcServices;
        public GrpcServer(GrpcServerBuilder builder, IGrpcServices services)
        {
            _builder = builder;
            _grpcServices = services;
            _options = builder.Options ?? throw new ArgumentNullException("GrpcServerOption cannot be null");
            _server = new Server();
            _server.Ports.Add(new ServerPort(_options.Host, _options.Port, ServerCredentials.Insecure));
        }

        public GrpcServer Start()
        {
            GrpcHandlerDirector.ConfigActor();
            var definition = _grpcServices.BindServices();
            definition = ResolveInterceptors(definition);
            _server.Services.Add(definition);
            _server.Start();
            return this;
        }

        private ServerServiceDefinition ResolveInterceptors(ServerServiceDefinition definition)
        {            
            if (_builder.Interceptors != null && _builder.Interceptors.Count > 0)
            {
                var interceptors = new List<Interceptor>();
                foreach (var interceptorType in _builder.Interceptors)
                {
                    var interceptor = (Interceptor)GrpcServerBuilder.ServiceProvider.GetService(interceptorType);
                    if (interceptor != null)
                    {
                        interceptors.Add(interceptor);
                    }
                }

                if (interceptors.Count > 0)
                {
                    definition = definition.Intercept(interceptors.ToArray());
                }
            }
            return definition;
        }

        public async Task ShutdownAsync(Func<Task> action = null)
        {
            await _server.ShutdownAsync();
            if (action != null)
            {
                await action.Invoke();
            }
        }
    }

    public class ProtoPropertyCompare : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            return string.CompareOrdinal(x, y);
        }
    }
}
