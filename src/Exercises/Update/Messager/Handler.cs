namespace Journal.Exercises.Update.Messager;

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
        if (message.exercise == null)
            return;

        // Build update data
        var exerciseData = new
        {
            name = message.exercise.Name,
            description = message.exercise.Description,
            type = message.exercise.Type,
            lastUpdated = message.exercise.LastUpdated
        };

        // ===== SYNC OPENSEARCH =====
        try
        {
            var openSearchResponse = await _openSearchClient.UpdateAsync<Databases.OpenSearch.Indexes.Exercise.Index, object>(
                message.exercise.Id.ToString(),
                u => u.Index("exercises")
                      .Doc(exerciseData)
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

                workout.Exercise.Name = message.exercise.Name;
                workout.Exercise.Description = message.exercise.Description;
                workout.Exercise.Type = message.exercise.Type;
                workout.Exercise.LastUpdated = message.exercise.LastUpdated;
                // Note: Muscles are intentionally not updated

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
        // No additional tables to sync for update operation
    }
}