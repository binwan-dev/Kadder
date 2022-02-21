using Kadder.Simple.Server.Protocols;

namespace Kadder.Simple.Server;

[KServicer]
public class NoteServicer
{
    public Task<CreateNoteResponse> CreateNoteAsync(CreateNoteRequest request)
    {
        Console.WriteLine($"Receive create note request[Title: {request.Title} -> Content: {request.Content}]");

        return Task.FromResult(new CreateNoteResponse() { Title = request.Title, StatusCode = true });
    }
}
