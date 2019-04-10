using System;
using Grpc.Core;
using System.Threading.Tasks;
using Atlantis.Grpc.Logging;
using Atlantis.Grpc.Middlewares;
using Atlantis.Grpc.Utilies;

namespace Atlantis.Grpc
{
    public class GrpcMessageServicer
    {
        private readonly GrpcHandlerBuilder _builder;
        private readonly ILogger _logger;
        private readonly IJsonSerializer _jsonSerializer;

        public GrpcMessageServicer()
        {
            _builder = ObjectContainer.Resolve<GrpcHandlerBuilder>();
            _logger = ObjectContainer.Resolve<ILoggerFactory>()
                .Create<GrpcMessageServicer>();
            _jsonSerializer=ObjectContainer.Resolve<IJsonSerializer>();
        }

        public async Task<TMessageResult> ProcessAsync<TMessage, TMessageResult>(
            TMessage message, ServerCallContext callContext)
            where TMessage : BaseMessage
            where TMessageResult : MessageResult, new()
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
                    return new TMessageResult()
                    {
                        Code = context.Result.Code,
                        Message = context.Result.Message
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Message execute failed! reason:{ex.Message} Request:{_jsonSerializer.Serialize(message)}", ex);
                return new TMessageResult() { Code = ResultCode.Exception, Message = "Service handle failed, Please try again!" };
            }

        }
    }
}
