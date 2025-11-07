namespace Journal.Exercises.Delete.Messager;

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
        // ===== SYNC OPENSEARCH =====
        try
        {
            await _openSearchClient.DeleteAsync<Databases.OpenSearch.Indexes.Exercise.Index>(
            message.Id.ToString(),
            d => d.Index("exercises")
            );
        }
        catch
        {
            Console.WriteLine($"Can't reach OpenSearch");
        }

        // ===== SYNC MONGODB =====
        try
        {
            // Delete the exercise document itself
            var exercise = await _mongoDbContext.Exercises
                .FirstOrDefaultAsync(e => e.Id == message.Id);

            if (exercise != null)
            {
                _mongoDbContext.Exercises.Remove(exercise);
                await _mongoDbContext.SaveChangesAsync();
                Console.WriteLine($"Deleted exercise {message.Id} from MongoDB");
            }
            else
            {
                Console.WriteLine($"Exercise {message.Id} not found in MongoDB");
            }

            // Handle workouts that reference this exercise
            var workouts = await _mongoDbContext.Workouts
                .Where(w => w.ExerciseId == message.Id)
                .ToListAsync();

            if (!workouts.Any())
            {
                Console.WriteLine($"No workouts found for exercise {message.Id}");
            }
            else
            {
                if (message.IsDeleteWorkouts)
                {
                    _mongoDbContext.Workouts.RemoveRange(workouts);
                    Console.WriteLine($"Deleted {workouts.Count} workout(s) for exercise {message.Id}");
                }
                else
                {
                    foreach (var workout in workouts)
                    {
                        workout.ExerciseId = Guid.Empty;
                        workout.Exercise = null;
                        workout.LastUpdated = DateTime.UtcNow;
                    }
                    _mongoDbContext.Workouts.UpdateRange(workouts);
                    Console.WriteLine($"Unlinked {workouts.Count} workout(s) from exercise {message.Id}");
                }

                await _mongoDbContext.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"MongoDB error: {ex.Message}");
            throw;
        }

        // ===== SYNC CONTEXT TABLES =====
        var exerciseMuscles = _context.ExerciseMuscles
            .Where(em => em.ExerciseId == message.Id);

        _context.ExerciseMuscles.RemoveRange(exerciseMuscles);
        await _context.SaveChangesAsync();
    }
}