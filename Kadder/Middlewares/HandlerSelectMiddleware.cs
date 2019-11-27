using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace Kadder.Middlewares
{
    public class HandlerSelectMiddleware : GrpcMiddlewareBase
    {
        public HandlerSelectMiddleware(HandlerDelegateAsync next) : base(next)
        {
        }

        protected override async Task DoHandleAsync(GrpcContext context)
        {
            using (var scope = GrpcServerBuilder.ServiceProvider.CreateScope())
            {
                context.Result = await context.Hander(context.Message, scope);
                context.Complete();
            }
        }

    }
}
