using System;
using System.Threading.Tasks;
using Atlantis.Grpc.Utilies;

namespace Atlantis.Grpc
{
    public interface IMessageServicerProxy
    {
        Func<TMessage, Task<TMessageResult>>
            GetHandleDelegate<TMessage, TMessageResult>(TMessage message)
            where TMessageResult : class where TMessage : BaseMessage;

        Func<TMessage, Task>
            GetHandleDelegate<TMessage>(TMessage message)
            where TMessage : BaseMessage;
    }
}
