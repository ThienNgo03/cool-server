namespace Journal.Exercises.Patch.Messager;

using OpenSearch.Client;
using System.Dynamic;

public class Handler
{
    private readonly JournalDbContext _context;
    private readonly IOpenSearchClient _openSearchClient;

    public Handler(JournalDbContext context, IOpenSearchClient openSearchClient)
    {
        _context = context;
        _openSearchClient = openSearchClient;
    }

    public async Task Handle(Message message)
    {
        if (message.changes != null && message.changes.Any())
        {
            var updateFields = new Dictionary<string, object?>();

            foreach (var (path, value) in message.changes)
            {
                var fieldName = path.TrimStart('/').ToLowerInvariant();

                if (fieldName == "name")
                {
                    updateFields["name"] = value;
                    continue;
                }

                if (fieldName == "description")
                {
                    updateFields["description"] = value;
                    continue;
                }

                if (fieldName == "type")
                {
                    updateFields["type"] = value;
                    continue;
                }
            }
            updateFields["lastUpdated"] = message.exercise.LastUpdated;

            if (updateFields.Any())
            {
                var updateResponse = await _openSearchClient.UpdateAsync<Databases.OpenSearch.Indexes.Exercise.Index, object>(
                    message.exercise.Id.ToString(),
                    u => u
                        .Index("exercises")
                        .Doc(updateFields)
                        .DocAsUpsert(false)
                );

                if (!updateResponse.IsValid)
                {
                    Console.WriteLine($"Error updating document in OpenSearch: {updateResponse.ServerError?.Error?.Reason ?? updateResponse.DebugInformation}");
                }
            }
        }
    }
}