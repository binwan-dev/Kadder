using System.Threading.Tasks;
using Atlantis.Grpc.Utilies;

namespace Atlantis.Grpc.Middlewares
{
    public class HandlerSelectMiddleware : GrpcMiddlewareBase
    {
        private readonly IMessageServicerProxy _proxy;

        public HandlerSelectMiddleware(HandlerDelegateAsync next) : base(next)
        {
            _proxy = ObjectContainer.Resolve<IMessageServicerProxy>();
        }

        protected override async Task DoHandleAsync(GrpcContext context)
        {
            var handle = _proxy
                .GetHandleDelegate<BaseMessage, MessageResult>(context.Message);
            context.Result = await handle(context.Message);
            context.HasDone = true;
        }

    }
}
