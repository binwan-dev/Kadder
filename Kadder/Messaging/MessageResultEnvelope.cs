using System;

namespace Kadder.Messaging
{
    public class MessageResultEnvelope<TMessageResult> : IMessageResultEnvelope
    {
        public TMessageResult MessageResult { get; set; }

        public bool IsEmpty { get; set; }
    }
}
