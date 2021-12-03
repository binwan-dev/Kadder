using ProtoBuf;

namespace Kadder.Simple.Protocol.Requests
{
    [ProtoContract]
    public class HelloRequest
    {
        [ProtoMember(1)]
        public string Msg { get; set; }
    }
}