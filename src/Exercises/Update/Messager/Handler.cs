namespace Journal.Exercises.Update.Messager;

using OpenSearch.Client;
using Microsoft.EntityFrameworkCore;

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
        if (message.exercise != null)
        {
            // Partial update - only update specific fields, don't touch muscles
            var updateResponse = await _openSearchClient.UpdateAsync<Databases.OpenSearch.Indexes.Exercise.Index, object>(
                message.exercise.Id.ToString(),
                u => u
                    .Index("exercises")
                    .Doc(new
                    {
                        name = message.exercise.Name,
                        description = message.exercise.Description,
                        type = message.exercise.Type,
                        lastUpdated = message.exercise.LastUpdated
                    })
                    .DocAsUpsert(false) // Don't create if doesn't exist
            );

            if (!updateResponse.IsValid)
            {
                Console.WriteLine($"Error updating document: {updateResponse.ServerError?.Error?.Reason ?? updateResponse.DebugInformation}");
            }
        }
    }
}