using Kadder.Simple.Duplex.Protocol;
using Kadder.Simple.Duplex.Protocol.Protocols;

namespace Kadder.Simple.Duplex.Server;

public class NoteServicer : INoteServicer
{
    public Task<CreateNoteResponse> CreateAsync(CreateNoteRequest request)
    {
        return Task.FromResult(new CreateNoteResponse()
        {
            Title = request.Title,
            Status = true
        });
    }
}
