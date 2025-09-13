using Grpc.Core;

namespace Journal.Notes.Search;

public class Service : SearchMethod.SearchMethodBase
{
    private readonly JournalDbContext _context;
    public Service(JournalDbContext context)
    {
        _context = context;
    }
    public override async Task<ListResult> Search(Parameter parameters, ServerCallContext context)
    {
        var query = _context.Notes.AsQueryable();

        if (!string.IsNullOrWhiteSpace(parameters.Id))
            query = query.Where(x => x.Id.ToString() == parameters.Id);

        if (!string.IsNullOrWhiteSpace(parameters.UserId))
            query = query.Where(x => x.UserId.ToString() == parameters.UserId);

        if (!string.IsNullOrWhiteSpace(parameters.JourneyId))
            query = query.Where(x => x.JourneyId.ToString() == parameters.JourneyId);

        if (!string.IsNullOrWhiteSpace(parameters.Content))
            query = query.Where(x => x.Content.Contains(parameters.Content));

        if (!string.IsNullOrWhiteSpace(parameters.Mood))
            query = query.Where(x => x.Mood == parameters.Mood);

        if (!string.IsNullOrWhiteSpace(parameters.Date) && DateTime.TryParse(parameters.Date, out var parsedDate))
            query = query.Where(x => x.Date.Date == parsedDate.Date);

        var result = await query.AsNoTracking().ToListAsync();
        var listResultProto = new ListResult();
        listResultProto.Results.AddRange(result.Select(x => new Result
        {
            Id = x.Id.ToString(),
            JourneyId = x.JourneyId.ToString(),
            UserId = x.UserId.ToString(),
            Content = x.Content,
            Mood = x.Mood,
            Date = x.Date.ToString("o")
        }));
        return listResultProto;
    }
}
