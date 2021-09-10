using System;

namespace Kadder.Messaging
{
    public interface IMessageEnvelope
    {
        bool IsEmpty { get; set; }
    }
}
