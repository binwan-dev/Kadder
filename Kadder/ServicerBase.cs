using Kadder.Messaging;
using Kadder.Streaming;
using Kadder.Utilies;

namespace Kadder
{
    public abstract class ServicerBase<TContext> : IMessagingServicer where TContext : IMessagingContext
    {
        private TContext _context;

        internal void SetContext(TContext context) => _context = context;

        protected TContext Context => _context;
    }
}
