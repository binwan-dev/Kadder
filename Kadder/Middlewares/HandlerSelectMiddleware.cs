using Kadder.Utilies;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace Kadder.Middlewares
{
    public class HandlerSelectMiddleware : GrpcMiddlewareBase
    {
        private readonly IMessageServicerProxy _proxy;

        public HandlerSelectMiddleware(HandlerDelegateAsync next) : base(next)
        {
            _proxy = GrpcServerBuilder.ServiceProvider.GetService<IMessageServicerProxy>();
        }

        protected override async Task DoHandleAsync(GrpcContext context)
        {
            using (var scope = GrpcServerBuilder.ServiceProvider.CreateScope())
            {
                var handle = _proxy.GetHandleDelegate<BaseMessage, MessageResult>(
                    context.Message, scope.ServiceProvider);
                context.Result = await handle(context.Message);
                context.HasDone = true;
            }
        }

    }
}
