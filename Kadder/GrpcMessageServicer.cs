using System;
using Grpc.Core;
using System.Threading.Tasks;
using Kadder.Middlewares;
using Microsoft.Extensions.Logging;
using Kadder.Utilies;
using Kadder.Messaging;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace Kadder
{
    public class GrpcMessageServicer
    {
        private readonly GrpcHandlerBuilder _builder;
        private readonly ILogger<GrpcMessageServicer> _log;

        public GrpcMessageServicer(ILogger<GrpcMessageServicer> log, GrpcHandlerBuilder builder)
        {
            _builder = builder;
            _log = log;
        }

        public async Task<IMessageResultEnvelope> ProcessAsync(GrpcContext context, Func<IMessageEnvelope, IServiceScope, Task<IMessageResultEnvelope>> handler)
        {
            try
            {
                context.Hander = handler;
                await _builder.DelegateProxyAsync(context);
                return context.Result;
            }
            catch (Exception ex)
            {
                _log.LogError(ex, $"Message execute failed! reason:{ex.GetExceptionMessage()} Request:{JsonSerializer.Serialize(context.Message)}");
                throw;
            }
        }
    }
}
