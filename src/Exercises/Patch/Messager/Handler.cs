namespace Journal.Exercises.Patch.Messager;

using Journal.Databases.MongoDb;
using OpenSearch.Client;

public class Handler
{
    private readonly JournalDbContext _context;
    private readonly IOpenSearchClient _openSearchClient;
    private readonly MongoDbContext _mongoDbContext;

    public Handler(
        JournalDbContext context,
        IOpenSearchClient openSearchClient,
        MongoDbContext mongoDbContext)
    {
        _context = context;
        _openSearchClient = openSearchClient;
        _mongoDbContext = mongoDbContext;
    }

    public async Task Handle(Message message)
    {
        if (message.changes == null || !message.changes.Any())
            return;

        // Build update data
        var updateFields = new Dictionary<string, object?>();
        var allowedFields = new HashSet<string> { "name", "description", "type" };

        foreach (var (path, value) in message.changes)
        {
            var fieldName = path.TrimStart('/').ToLowerInvariant();
            if (allowedFields.Contains(fieldName))
            {
                updateFields[fieldName] = value;
            }
        }

        if (!updateFields.Any())
            return;

        updateFields["lastUpdated"] = message.exercise.LastUpdated;

        // ===== SYNC OPENSEARCH =====
        try 
        {
            var openSearchResponse = await _openSearchClient.UpdateAsync<Databases.OpenSearch.Indexes.Exercise.Index, object>(
                message.exercise.Id.ToString(),
                u => u.Index("exercises")
                      .Doc(updateFields)
                      .DocAsUpsert(false)
            );
        }
        catch
        {
            Console.WriteLine($"Can't reach OpenSearch");
        }

        // ===== SYNC MONGODB =====
        try
        {
            var workouts = await _mongoDbContext.Workouts
                .Where(w => w.ExerciseId == message.exercise.Id)
                .ToListAsync();

            if (!workouts.Any())
            {
                Console.WriteLine($"No workouts found for exercise {message.exercise.Id}");
                return;
            }

            foreach (var workout in workouts)
            {
                if (workout.Exercise == null)
                    continue;

                foreach (var change in message.changes)
                {
                    var fieldName = change.Path.TrimStart('/').ToLowerInvariant();
                    var value = change.Value?.ToString();

                    switch (fieldName)
                    {
                        case "name":
                            workout.Exercise.Name = value;
                            break;
                        case "description":
                            workout.Exercise.Description = value;
                            break;
                        case "type":
                            workout.Exercise.Type = value;
                            break;
                    }
                }

                workout.LastUpdated = DateTime.UtcNow;
            }

            _mongoDbContext.Workouts.UpdateRange(workouts);
            await _mongoDbContext.SaveChangesAsync();

            Console.WriteLine($"Updated {workouts.Count} workout(s) for exercise {message.exercise.Id}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"MongoDB error: {ex.Message}");
            throw;
        }

        // ===== SYNC CONTEXT TABLES =====
        // No additional tables to sync for patch operation
    }
}