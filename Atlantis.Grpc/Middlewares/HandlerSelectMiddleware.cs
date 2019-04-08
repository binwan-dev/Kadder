using Followme.AspNet.Core.FastCommon.Commanding;
using Followme.AspNet.Core.FastCommon.Querying;
using Followme.AspNet.Core.FastCommon.Components;
using Followme.AspNet.Core.FastCommon.Infrastructure;
using System.Threading.Tasks;
using System;

namespace Followme.AspNet.Core.FastCommon.ThirdParty.GrpcServer.Middlewares
{
    public class HandlerSelectMiddleware:GrpcMiddlewareBase
    {
        private readonly ICommandHandler _commandHandler;
        private readonly IQueryServicerDelegateFactory _queryDelegateFactory;
        
        public HandlerSelectMiddleware(HandlerDelegateAsync next):base(next)
        {
            _commandHandler=ObjectContainer.Resolve<ICommandHandler>();
            _queryDelegateFactory=ObjectContainer.Resolve<IQueryServicerDelegateFactory>();
        }

        protected override async Task DoHandleAsync(GrpcContext context)
        {
            switch(context.Message.GetMessageExecutingType())
            {
                case MessageExecutingType.Command:await HandleCommandAsync(context);return;
                case MessageExecutingType.Query:await HandleQueryAsync(context);return;
            }
        }

        private async Task HandleCommandAsync(GrpcContext context)
        {
            context.Result=await _commandHandler.Handle<BaseMessage,MessageResult>(context.Message);
            context.HasDone=true;
        }

        private async Task HandleQueryAsync(GrpcContext context)
        {
            context.Result=await _queryDelegateFactory.GetHandleDelegateAsync<BaseMessage,MessageResult>(context.Message)(context.Message);
            context.HasDone=true;
        }
    }
}
