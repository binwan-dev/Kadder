using System;

namespace Kadder.Messaging
{
    public class MessageEnvelope<TMessage> : IMessageEnvelope
    {
        public TMessage Message { get; set; }

        public bool IsEmpty { get; set; }
    }
}
