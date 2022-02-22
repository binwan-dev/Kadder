using ProtoBuf;

namespace Kadder.Simple.Duplex.Protocol.Protocols;

[ProtoContract]
public class CreateNoteRequest
{
    [ProtoMember(1)]
    public string Title { get; set; } = null!;

    [ProtoMember(2)]
    public string Content { get; set; } = null!;
}
