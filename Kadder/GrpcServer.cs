using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Kadder.Middlewares;
using Microsoft.Extensions.Options;

namespace Kadder
{
    public class GrpcServer
    {
        private readonly Server _server;
        private readonly GrpcServerOptions _options;

        public GrpcServer(
            GrpcServerBuilder builder, IGrpcServices services)
        {
            _options = builder.Options ?? throw new ArgumentNullException("GrpcServerOption cannot be null");
            _server = new Server();
            _server.Ports.Add(
                new ServerPort(
                    _options.Host, _options.Port, ServerCredentials.Insecure
                )
            );
            _server.Services.Add(services.BindServices());
        }

        public GrpcServer Start()
        {
            GrpcHandlerDirector.ConfigActor();
            _server.Start();
            return this;
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
