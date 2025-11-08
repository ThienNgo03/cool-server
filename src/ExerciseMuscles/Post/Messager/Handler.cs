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

        var mongoMuscle = new Journal.Databases.MongoDb.Collections.Exercise.Muscle
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

        // ===== SYNC MONGODB EXERCISES COLLECTION =====
        try
        {
            var mongoExercise = await _mongoDbContext.Exercises
                .FirstOrDefaultAsync(e => e.Id == message.exerciseMuscles.ExerciseId);

            if (mongoExercise == null)
            {
                Console.WriteLine($"Exercise {message.exerciseMuscles.ExerciseId} not found in MongoDB");
                return;
            }

            if (mongoExercise.Muscles == null)
            {
                mongoExercise.Muscles = new List<Journal.Databases.MongoDb.Collections.Exercise.Muscle>();
            }

            if (!mongoExercise.Muscles.Any(m => m.Id == mongoMuscle.Id))
            {
                mongoExercise.Muscles.Add(mongoMuscle);
                mongoExercise.LastUpdated = DateTime.UtcNow;

                _mongoDbContext.Exercises.Update(mongoExercise);
                await _mongoDbContext.SaveChangesAsync();

                Console.WriteLine($"Added muscle {muscle.Id} to exercise {mongoExercise.Id} in MongoDB");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"MongoDB Exercises error: {ex.Message}");
            throw;
        }

        // ===== SYNC MONGODB WORKOUTS COLLECTION =====
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

            var workoutMuscle = new Journal.Databases.MongoDb.Collections.Workout.Muscle
            {
                Id = muscle.Id,
                Name = muscle.Name,
                CreatedDate = muscle.CreatedDate,
                LastUpdated = muscle.LastUpdated
            };

            foreach (var workout in workouts)
            {
                if (workout.Exercise == null)
                    continue;

                if (workout.Exercise.Muscles == null)
                {
                    workout.Exercise.Muscles = new List<Journal.Databases.MongoDb.Collections.Workout.Muscle>();
                }

                if (!workout.Exercise.Muscles.Any(m => m.Id == workoutMuscle.Id))
                {
                    workout.Exercise.Muscles.Add(workoutMuscle);
                    workout.LastUpdated = DateTime.UtcNow;
                }
            }

            _mongoDbContext.Workouts.UpdateRange(workouts);
            await _mongoDbContext.SaveChangesAsync();

            Console.WriteLine($"Added muscle {muscle.Id} to {workouts.Count} workout(s)");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"MongoDB Workouts error: {ex.Message}");
            throw;
        }

        #region Cassandra Sync
        //// ===== SYNC CASSANDRA =====
        //var exerciseMuscleByExerciseId = new ExerciseMuscles.Tables.CassandraTables.ByExerciseIds.Table
        //{
        //    Id = message.exerciseMuscles.Id,
        //    ExerciseId = message.exerciseMuscles.ExerciseId,
        //    MuscleId = message.exerciseMuscles.MuscleId,
        //    CreatedDate = message.exerciseMuscles.CreatedDate,
        //    LastUpdated = message.exerciseMuscles.LastUpdated
        //};
        //await _cassandraContext.ExerciseMuscleByExerciseIds.Insert(exerciseMuscleByExerciseId).ExecuteAsync();
        //var exerciseMuscleByMuscleId = new ExerciseMuscles.Tables.CassandraTables.ByMuscleIds.Table
        //{
        //    Id = message.exerciseMuscles.Id,
        //    ExerciseId = message.exerciseMuscles.ExerciseId,
        //    MuscleId = message.exerciseMuscles.MuscleId,
        //    CreatedDate = message.exerciseMuscles.CreatedDate,
        //    LastUpdated = message.exerciseMuscles.LastUpdated
        //};
        //await _cassandraContext.ExerciseMuscleByMuscleIds.Insert(exerciseMuscleByMuscleId).ExecuteAsync();
        #endregion
        // ===== SYNC CONTEXT TABLES =====
        // No additional tables to sync for post operation
    }
}