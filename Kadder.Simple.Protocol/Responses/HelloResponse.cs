using ProtoBuf;

namespace Kadder.Simple.Protocol.Responses
{
    [ProtoContract]
    public class HelloResponse
    {
        [ProtoMember(1)]
        public string Msg { get; set; }
    }
}