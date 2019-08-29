using System;
using Grpc.Core;
using System.Threading.Tasks;
using Kadder.Middlewares;
using Microsoft.Extensions.Logging;
using Kadder.Utilies;

namespace Kadder
{
    public class GrpcMessageServicer
    {
        private readonly GrpcHandlerBuilder _builder;
        private readonly ILogger<GrpcMessageServicer> _log;
        private readonly IJsonSerializer _jsonSerializer;

        public GrpcMessageServicer(
            ILogger<GrpcMessageServicer> log,
            GrpcHandlerBuilder builder,
            IJsonSerializer jsonSerializer)
        {
            _builder = builder;
            _log = log;
            _jsonSerializer=jsonSerializer;
        }

        public async Task<TMessageResult> ProcessAsync<TMessage, TMessageResult>(
            TMessage message, ServerCallContext callContext)
            where TMessage : BaseMessage
            where TMessageResult :MessageResult, new()
        {
            try
            {
                var context = new GrpcContext(message, callContext);
                await _builder.DelegateProxyAsync(context);
                if (context.Result is TMessageResult)
                {
                    return (TMessageResult)context.Result;
                } 
                else
                {
                    throw new InvalidOperationException("Cannot convert message result!");
                }
            }
            catch (Exception ex)
            {
                _log.LogError(ex,$"Message execute failed! reason:{ex.GetExceptionMessage()} Request:{_jsonSerializer.Serialize(message)}");
                throw;
            }

        }
    }
}
