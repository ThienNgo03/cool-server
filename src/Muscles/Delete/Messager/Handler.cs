namespace Journal.Muscles.Delete.Messager;

using Journal.Databases;
using Journal.Databases.MongoDb;
using Microsoft.EntityFrameworkCore;
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
        // Find all exercises that have this muscle
        var exerciseIds = await _context.ExerciseMuscles
            .Where(em => em.MuscleId == message.muscleId)
            .Select(em => em.ExerciseId)
            .Distinct()
            .ToListAsync();

        if (!exerciseIds.Any())
        {
            Console.WriteLine($"No exercises found with muscle {message.muscleId}.");
        }
        else
        {
            // ===== SYNC OPENSEARCH =====
            try
            {
                foreach (var exerciseId in exerciseIds)
                {
                    await RemoveMuscleFromExerciseOpenSearch(exerciseId, message.muscleId);
                }
            }
            catch
            {
                Console.WriteLine($"Can't reach OpenSearch");
            }
            

            // ===== SYNC MONGODB =====
            try
            {
                var workouts = await _mongoDbContext.Workouts
                    .Where(w => exerciseIds.Contains(w.ExerciseId))
                    .ToListAsync();

                if (!workouts.Any())
                {
                    Console.WriteLine($"No workouts found for exercises with muscle {message.muscleId}");
                }
                else
                {
                    int updatedCount = 0;
                    foreach (var workout in workouts)
                    {
                        if (workout.Exercise?.Muscles == null || !workout.Exercise.Muscles.Any())
                            continue;

                        var initialCount = workout.Exercise.Muscles.Count;
                        workout.Exercise.Muscles.RemoveAll(m => m.Id == message.muscleId);

                        if (workout.Exercise.Muscles.Count < initialCount)
                        {
                            workout.LastUpdated = DateTime.UtcNow;
                            updatedCount++;
                        }
                    }

                    if (updatedCount > 0)
                    {
                        _mongoDbContext.Workouts.UpdateRange(workouts.Where(w =>
                            w.Exercise?.Muscles != null));
                        await _mongoDbContext.SaveChangesAsync();
                        Console.WriteLine($"Removed muscle from {updatedCount} workout(s)");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"MongoDB error: {ex.Message}");
                throw;
            }
        }

        // ===== SYNC CONTEXT TABLES =====
        var exerciseMuscles = _context.ExerciseMuscles
            .Where(em => em.MuscleId == message.muscleId);
        _context.ExerciseMuscles.RemoveRange(exerciseMuscles);
        await _context.SaveChangesAsync();
    }

    private async Task RemoveMuscleFromExerciseOpenSearch(Guid exerciseId, Guid muscleId)
    {
        var getResponse = await _openSearchClient.GetAsync<Databases.OpenSearch.Indexes.Exercise.Index>(
            exerciseId.ToString(),
            g => g.Index("exercises")
        );

        if (!getResponse.IsValid)
        {
            Console.WriteLine($"Error retrieving exercise {exerciseId}: {getResponse.ServerError?.Error?.Reason ?? getResponse.DebugInformation}");
            return;
        }

        var exerciseDoc = getResponse.Source;
        if (exerciseDoc.Muscles == null || !exerciseDoc.Muscles.Any())
        {
            Console.WriteLine($"No muscles found in exercise {exerciseId}.");
            return;
        }

        var initialCount = exerciseDoc.Muscles.Count;
        exerciseDoc.Muscles.RemoveAll(m => m.Id == muscleId);

        if (exerciseDoc.Muscles.Count < initialCount)
        {
            var updateResponse = await _openSearchClient.UpdateAsync<Databases.OpenSearch.Indexes.Exercise.Index, object>(
                exerciseId.ToString(),
                u => u
                    .Index("exercises")
                    .Doc(new
                    {
                        muscles = exerciseDoc.Muscles,
                        lastUpdated = DateTime.UtcNow
                    })
                    .DocAsUpsert(false)
            );

            if (!updateResponse.IsValid)
            {
                Console.WriteLine($"Error removing muscle from exercise {exerciseId}: {updateResponse.ServerError?.Error?.Reason ?? updateResponse.DebugInformation}");
            }
        }
    }
}