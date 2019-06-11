using System;
using System.Threading.Tasks;
using Kadder.Utilies;

namespace Kadder
{
    public interface IMessageServicerProxy
    {
        Func<TMessage, Task<TMessageResult>>
            GetHandleDelegate<TMessage, TMessageResult>
        (
            TMessage message,
            IServiceProvider serviceProvider
        )
            where TMessageResult : class where TMessage : BaseMessage;

        Func<TMessage, Task> GetHandleDelegate<TMessage>(
            TMessage message, IServiceProvider serviceProvider)
            where TMessage : BaseMessage;
    }
}
