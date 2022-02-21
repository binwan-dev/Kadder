using ProtoBuf;

namespace Kadder.Simple.Server.Protocols;

[ProtoContract(ImplicitFields=ImplicitFields.AllPublic)]
public class CreateNoteResponse
{
    public string Title { get; set; }

    public bool StatusCode { get; set; }
}
