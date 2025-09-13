using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace Journal.Notes.Create;

public class Service:CreateMethod.CreateMethodBase
{
    private readonly JournalDbContext _context;
    public Service(JournalDbContext context)
    {
        _context = context;
    }
    public override async Task<Empty> Create(Payload payload, ServerCallContext context)
    {
        var userId = Guid.NewGuid();
        var journeyId = Guid.NewGuid();
        var note = new Databases.Journal.Tables.Note.Table 
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            JourneyId = journeyId,
            Content = payload.Content,
            Date = DateTime.UtcNow,
            Mood = payload.Mood
        };
        _context.Notes.Add(note); 
        await _context.SaveChangesAsync(); 
        return new Empty();
    }
}
