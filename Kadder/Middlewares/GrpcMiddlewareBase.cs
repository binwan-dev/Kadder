using System;
using System.Threading.Tasks;

namespace Kadder.Middlewares
{
    public abstract class GrpcMiddlewareBase
    {
        private readonly HandlerDelegateAsync _next;
        private readonly IServiceProvider _serviceProvider;

        public GrpcMiddlewareBase(HandlerDelegateAsync next)
        {
            _next = next;
            Sort = 0;
            _serviceProvider = GrpcServerBuilder.ServiceProvider;
        }

        public virtual int Sort { get; set; }

        protected IServiceProvider ServiceProvider => _serviceProvider;

        public virtual async Task HandleAsync(GrpcContext context)
        {
            await DoHandleAsync(context);
            if (!context.IsDone) await _next(context);
            await DoHandleResultAsync(context);
        }

        protected abstract Task DoHandleAsync(GrpcContext context);

        protected virtual Task DoHandleResultAsync(GrpcContext context)
        {
            return Task.CompletedTask;
        }
    }
}
