namespace Journal.ExerciseMuscles.Delete.Messager;

using Cassandra.Data.Linq;
using Journal.Databases.MongoDb;
using OpenSearch.Client;

public class Handler
{
    private readonly JournalDbContext _context;
    private readonly IOpenSearchClient _openSearchClient;
    private readonly MongoDbContext _mongoDbContext;
    private readonly Databases.CassandraCql.Context _cassandraContext;
    public Handler(
        JournalDbContext context,
        IOpenSearchClient openSearchClient,
        MongoDbContext mongoDbContext,
        Databases.CassandraCql.Context cassandraContext)
    {
        _context = context;
        _openSearchClient = openSearchClient;
        _mongoDbContext = mongoDbContext;
        _cassandraContext = cassandraContext;
    }

    public async Task Handle(Message message)
    {
        // ===== SYNC OPENSEARCH =====
        try
        {
            var getResponse = await _openSearchClient.GetAsync<Databases.OpenSearch.Indexes.Exercise.Index>(
                message.exerciseId.ToString(),
                g => g.Index("exercises")
            );

            if (!getResponse.IsValid)
            {
                Console.WriteLine($"OpenSearch error: {getResponse.ServerError?.Error?.Reason ?? getResponse.DebugInformation}");
                return;
            }

            var exerciseDoc = getResponse.Source;

            if (exerciseDoc.Muscles == null || !exerciseDoc.Muscles.Any())
            {
                Console.WriteLine($"No muscles found for exercise {message.exerciseId}");
                return;
            }

            var muscleToRemove = exerciseDoc.Muscles.FirstOrDefault(m => m.Id == message.muscleId);

            if (muscleToRemove != null)
            {
                exerciseDoc.Muscles.Remove(muscleToRemove);

                var updateResponse = await _openSearchClient.UpdateAsync<Databases.OpenSearch.Indexes.Exercise.Index, object>(
                    message.exerciseId.ToString(),
                    u => u.Index("exercises")
                          .Doc(new
                          {
                              muscles = exerciseDoc.Muscles,
                              lastUpdated = DateTime.UtcNow
                          })
                          .DocAsUpsert(false)
                );

                if (!updateResponse.IsValid)
                {
                    Console.WriteLine($"OpenSearch error: {updateResponse.ServerError?.Error?.Reason ?? updateResponse.DebugInformation}");
                }
            }
            else
            {
                Console.WriteLine($"Muscle {message.muscleId} not found in exercise {message.exerciseId}");
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
                .Where(w => w.ExerciseId == message.exerciseId)
                .ToListAsync();

            if (!workouts.Any())
            {
                Console.WriteLine($"No workouts found for exercise {message.exerciseId}");
                return;
            }

            foreach (var workout in workouts)
            {
                if (workout.Exercise?.Muscles != null)
                {
                    var initialCount = workout.Exercise.Muscles.Count;
                    workout.Exercise.Muscles.RemoveAll(m => m.Id == message.muscleId);

                    if (workout.Exercise.Muscles.Count < initialCount)
                    {
                        workout.LastUpdated = DateTime.UtcNow;
                    }
                }
            }

            _mongoDbContext.Workouts.UpdateRange(workouts);
            await _mongoDbContext.SaveChangesAsync();

            Console.WriteLine($"Removed muscle {message.muscleId} from {workouts.Count} workout(s)");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"MongoDB error: {ex.Message}");
            throw;
        }

        #region Cassandra Sync
        //// ===== SYNC CASSANDRA =====
        //await _cassandraContext.ExerciseMuscleByExerciseIds
        //    .Where(x=>x.ExerciseId==message.exerciseId&&x.Id==message.id)
        //    .Delete()
        //    .ExecuteAsync();
        //await _cassandraContext.ExerciseMuscleByMuscleIds
        //    .Where(x=>x.MuscleId==message.muscleId&&x.Id==message.id)
        //    .Delete()
        //    .ExecuteAsync();
        #endregion
        // ===== SYNC CONTEXT TABLES =====
        // No additional tables to sync for delete operation
    }
}