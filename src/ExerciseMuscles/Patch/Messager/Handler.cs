namespace Journal.ExerciseMuscles.Patch.Messager;

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
        if (message.changes == null || !message.changes.Any())
            return;

        Guid newMuscleId = Guid.Empty;
        Guid newExerciseId = Guid.Empty;

        // Parse changes
        var muscleIdChange = message.changes.FirstOrDefault(c =>
            c.Path.TrimStart('/').Equals("MuscleId", StringComparison.OrdinalIgnoreCase));

        var exerciseIdChange = message.changes.FirstOrDefault(c =>
            c.Path.TrimStart('/').Equals("ExerciseId", StringComparison.OrdinalIgnoreCase));

        var hasMuscleIdChange = muscleIdChange.Path != null && Guid.TryParse(muscleIdChange.Value?.ToString(), out newMuscleId);
        var hasExerciseIdChange = exerciseIdChange.Path != null && Guid.TryParse(exerciseIdChange.Value?.ToString(), out newExerciseId);

        if (!hasMuscleIdChange && !hasExerciseIdChange)
            return;

        // Determine which IDs to use
        var targetMuscleId = hasMuscleIdChange ? newMuscleId : message.entity.MuscleId;
        var targetExerciseId = hasExerciseIdChange ? newExerciseId : message.entity.ExerciseId;

        // ===== GET MUSCLE DATA FROM CONTEXT =====
        var muscle = await _context.Muscles.FirstOrDefaultAsync(x => x.Id == targetMuscleId);

        if (muscle == null)
        {
            Console.WriteLine($"Muscle {targetMuscleId} not found");
            return;
        }

        // Build muscle data for both OpenSearch and MongoDB
        var openSearchMuscle = new Databases.OpenSearch.Indexes.Muscle.Index
        {
            Id = muscle.Id,
            Name = muscle.Name,
            CreatedDate = muscle.CreatedDate,
            LastUpdated = muscle.LastUpdated
        };

        var mongoMuscle = new Journal.Databases.MongoDb.Collections.Workout.Muscle
        {
            Id = muscle.Id,
            Name = muscle.Name,
            CreatedDate = muscle.CreatedDate,
            LastUpdated = muscle.LastUpdated
        };

        // ===== SYNC OPENSEARCH =====
        try
        {
            // Remove muscle from old exercise
            var getOldResponse = await _openSearchClient.GetAsync<Databases.OpenSearch.Indexes.Exercise.Index>(
                message.oldExerciseId.ToString(),
                g => g.Index("exercises")
            );

            if (getOldResponse.IsValid)
            {
                var oldExerciseDoc = getOldResponse.Source;

                if (oldExerciseDoc.Muscles != null && oldExerciseDoc.Muscles.Any())
                {
                    var muscleToRemove = oldExerciseDoc.Muscles.FirstOrDefault(m => m.Id == message.oldMuscleId);

                    if (muscleToRemove != null)
                    {
                        oldExerciseDoc.Muscles.Remove(muscleToRemove);

                        var updateOldResponse = await _openSearchClient.UpdateAsync<Databases.OpenSearch.Indexes.Exercise.Index, object>(
                            message.oldExerciseId.ToString(),
                            u => u.Index("exercises")
                                  .Doc(new
                                  {
                                      muscles = oldExerciseDoc.Muscles,
                                      lastUpdated = DateTime.UtcNow
                                  })
                                  .DocAsUpsert(false)
                        );
                    }
                }
            }

            // Add muscle to new exercise
            var getNewResponse = await _openSearchClient.GetAsync<Databases.OpenSearch.Indexes.Exercise.Index>(
                targetExerciseId.ToString(),
                g => g.Index("exercises")
            );

            if (getNewResponse.IsValid)
            {
                var newExerciseDoc = getNewResponse.Source;

                if (newExerciseDoc.Muscles == null)
                {
                    newExerciseDoc.Muscles = new List<Databases.OpenSearch.Indexes.Muscle.Index>();
                }

                if (!newExerciseDoc.Muscles.Any(m => m.Id == openSearchMuscle.Id))
                {
                    newExerciseDoc.Muscles.Add(openSearchMuscle);

                    var updateNewResponse = await _openSearchClient.UpdateAsync<Databases.OpenSearch.Indexes.Exercise.Index, object>(
                        targetExerciseId.ToString(),
                        u => u.Index("exercises")
                              .Doc(new
                              {
                                  muscles = newExerciseDoc.Muscles,
                                  lastUpdated = DateTime.UtcNow
                              })
                              .DocAsUpsert(false)
                    );

                    if (!updateNewResponse.IsValid)
                    {
                        Console.WriteLine($"OpenSearch error: {updateNewResponse.ServerError?.Error?.Reason ?? updateNewResponse.DebugInformation}");
                    }
                }
            }
        }
        catch
        {
            Console.WriteLine($"Can't reach OpenSearch");
        }

        // ===== SYNC MONGODB =====
        try
        {
            // Remove muscle from old exercise's workouts
            var oldWorkouts = await _mongoDbContext.Workouts
                .Where(w => w.ExerciseId == message.oldExerciseId)
                .ToListAsync();

            if (oldWorkouts.Any())
            {
                foreach (var workout in oldWorkouts)
                {
                    if (workout.Exercise?.Muscles != null)
                    {
                        var initialCount = workout.Exercise.Muscles.Count;
                        workout.Exercise.Muscles.RemoveAll(m => m.Id == message.oldMuscleId);

                        if (workout.Exercise.Muscles.Count < initialCount)
                        {
                            workout.LastUpdated = DateTime.UtcNow;
                        }
                    }
                }

                _mongoDbContext.Workouts.UpdateRange(oldWorkouts);
                await _mongoDbContext.SaveChangesAsync();

                Console.WriteLine($"Removed muscle {message.oldMuscleId} from {oldWorkouts.Count} workout(s)");
            }

            // Add muscle to new exercise's workouts
            var newWorkouts = await _mongoDbContext.Workouts
                .Where(w => w.ExerciseId == targetExerciseId)
                .ToListAsync();

            if (newWorkouts.Any())
            {
                foreach (var workout in newWorkouts)
                {
                    if (workout.Exercise == null)
                        continue;

                    if (workout.Exercise.Muscles == null)
                    {
                        workout.Exercise.Muscles = new List<Journal.Databases.MongoDb.Collections.Workout.Muscle>();
                    }

                    if (!workout.Exercise.Muscles.Any(m => m.Id == mongoMuscle.Id))
                    {
                        workout.Exercise.Muscles.Add(mongoMuscle);
                        workout.LastUpdated = DateTime.UtcNow;
                    }
                }

                _mongoDbContext.Workouts.UpdateRange(newWorkouts);
                await _mongoDbContext.SaveChangesAsync();

                Console.WriteLine($"Added muscle {targetMuscleId} to {newWorkouts.Count} workout(s)");
            }
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