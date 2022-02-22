using ProtoBuf;

namespace Kadder.Simple.Duplex.Protocol.Protocols;

[ProtoContract]
public class CreateNoteResponse
{
    [ProtoMember(1)]
    public string Title { get; set; } = null!;

    [ProtoMember(2)]
    public bool Status { get; set; } 
}
