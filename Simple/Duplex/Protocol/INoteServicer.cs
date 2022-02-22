using Kadder.Simple.Duplex.Protocol.Protocols;

namespace Kadder.Simple.Duplex.Protocol;

[KServicer]
public interface INoteServicer
{
    Task<CreateNoteResponse> CreateAsync(CreateNoteRequest request);
}
