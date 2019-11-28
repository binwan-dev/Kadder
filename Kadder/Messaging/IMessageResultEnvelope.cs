using System;

namespace Kadder.Messaging
{
    public interface IMessageResultEnvelope
    {
        bool IsEmpty { get; set; }
    }
}
