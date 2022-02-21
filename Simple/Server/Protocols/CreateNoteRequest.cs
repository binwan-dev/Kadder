using ProtoBuf;

namespace Kadder.Simple.Server.Protocols;

[ProtoContract(ImplicitFields=ImplicitFields.AllPublic)]
public class CreateNoteRequest
{
    public string Title { get; set; }

    public string Content { get; set; }
}
