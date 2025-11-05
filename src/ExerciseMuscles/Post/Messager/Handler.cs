namespace Journal.ExerciseMuscles.Post.Messager;

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
        // ===== GET MUSCLE DATA FROM CONTEXT =====
        var muscle = await _context.Muscles.FirstOrDefaultAsync(x => x.Id == message.exerciseMuscles.MuscleId);

        if (muscle == null)
        {
            Console.WriteLine($"Muscle {message.exerciseMuscles.MuscleId} not found");
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
            var getResponse = await _openSearchClient.GetAsync<Databases.OpenSearch.Indexes.Exercise.Index>(
                message.exerciseMuscles.ExerciseId.ToString(),
                g => g.Index("exercises")
            );

            var exerciseDoc = getResponse.Source;

            if (exerciseDoc.Muscles == null)
            {
                exerciseDoc.Muscles = new List<Databases.OpenSearch.Indexes.Muscle.Index>();
            }

            if (!exerciseDoc.Muscles.Any(m => m.Id == openSearchMuscle.Id))
            {
                exerciseDoc.Muscles.Add(openSearchMuscle);

                var updateResponse = await _openSearchClient.UpdateAsync<Databases.OpenSearch.Indexes.Exercise.Index, object>(
                    message.exerciseMuscles.ExerciseId.ToString(),
                    u => u.Index("exercises")
                          .Doc(new
                          {
                              muscles = exerciseDoc.Muscles,
                              lastUpdated = message.exerciseMuscles.CreatedDate
                          })
                          .DocAsUpsert(false)
                );
            }
        }
        catch
        {
            Console.WriteLine($"OpenSearch Error");
        }

        // ===== SYNC MONGODB =====
        try
        {
            var workouts = await _mongoDbContext.Workouts
                .Where(w => w.ExerciseId == message.exerciseMuscles.ExerciseId)
                .ToListAsync();

            if (!workouts.Any())
            {
                Console.WriteLine($"No workouts found for exercise {message.exerciseMuscles.ExerciseId}");
                return;
            }

            foreach (var workout in workouts)
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

            _mongoDbContext.Workouts.UpdateRange(workouts);
            await _mongoDbContext.SaveChangesAsync();

            Console.WriteLine($"Added muscle {muscle.Id} to {workouts.Count} workout(s)");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"MongoDB error: {ex.Message}");
            throw;
        }

        // ===== SYNC CONTEXT TABLES =====
        // No additional tables to sync for post operation
    }
}