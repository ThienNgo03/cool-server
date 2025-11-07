namespace Journal.Muscles.Update.Messager;

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
            return;
        }

        // ===== SYNC OPENSEARCH =====
        try
        {
            foreach (var exerciseId in exerciseIds)
            {
                await UpdateMuscleInExerciseOpenSearch(exerciseId, message.updatedMuscle);
            }
        }
        catch
        {
            Console.WriteLine($"Can't reach OpenSearch");
        }

        // ===== SYNC MONGODB EXERCISES COLLECTION =====
        try
        {
            var exercises = await _mongoDbContext.Exercises
                .Where(e => exerciseIds.Contains(e.Id))
                .ToListAsync();

            if (!exercises.Any())
            {
                Console.WriteLine($"No exercises found in MongoDB with muscle {message.muscleId}");
            }
            else
            {
                int updatedCount = 0;
                foreach (var exercise in exercises)
                {
                    if (exercise.Muscles == null || !exercise.Muscles.Any())
                        continue;

                    var muscleToUpdate = exercise.Muscles.FirstOrDefault(m => m.Id == message.updatedMuscle.Id);
                    if (muscleToUpdate != null)
                    {
                        muscleToUpdate.Name = message.updatedMuscle.Name;
                        muscleToUpdate.LastUpdated = message.updatedMuscle.LastUpdated;
                        muscleToUpdate.CreatedDate = message.updatedMuscle.CreatedDate;

                        exercise.LastUpdated = DateTime.UtcNow;
                        updatedCount++;
                    }
                }

                if (updatedCount > 0)
                {
                    _mongoDbContext.Exercises.UpdateRange(exercises.Where(e =>
                        e.Muscles?.Any(m => m.Id == message.updatedMuscle.Id) == true));
                    await _mongoDbContext.SaveChangesAsync();
                    Console.WriteLine($"Updated muscle in {updatedCount} exercise(s) in MongoDB");
                }
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
                .Where(w => exerciseIds.Contains(w.ExerciseId))
                .ToListAsync();

            if (!workouts.Any())
            {
                Console.WriteLine($"No workouts found for exercises with muscle {message.muscleId}");
                return;
            }

            int updatedCount = 0;
            foreach (var workout in workouts)
            {
                if (workout.Exercise?.Muscles == null || !workout.Exercise.Muscles.Any())
                    continue;

                var muscleToUpdate = workout.Exercise.Muscles.FirstOrDefault(m => m.Id == message.updatedMuscle.Id);
                if (muscleToUpdate != null)
                {
                    muscleToUpdate.Name = message.updatedMuscle.Name;
                    muscleToUpdate.LastUpdated = message.updatedMuscle.LastUpdated;
                    muscleToUpdate.CreatedDate = message.updatedMuscle.CreatedDate;

                    workout.LastUpdated = DateTime.UtcNow;
                    updatedCount++;
                }
            }

            if (updatedCount > 0)
            {
                _mongoDbContext.Workouts.UpdateRange(workouts.Where(w =>
                    w.Exercise?.Muscles?.Any(m => m.Id == message.updatedMuscle.Id) == true));
                await _mongoDbContext.SaveChangesAsync();
                Console.WriteLine($"Updated muscle in {updatedCount} workout(s)");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"MongoDB Workouts error: {ex.Message}");
            throw;
        }
    }

    private async Task UpdateMuscleInExerciseOpenSearch(Guid exerciseId, Table updatedMuscle)
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

        var muscleToUpdate = exerciseDoc.Muscles.FirstOrDefault(m => m.Id == updatedMuscle.Id);
        if (muscleToUpdate != null)
        {
            muscleToUpdate.Name = updatedMuscle.Name;
            muscleToUpdate.LastUpdated = updatedMuscle.LastUpdated;
            muscleToUpdate.CreatedDate = updatedMuscle.CreatedDate;

            var updateResponse = await _openSearchClient.UpdateAsync<Databases.OpenSearch.Indexes.Exercise.Index, object>(
                exerciseId.ToString(),
                u => u
                    .Index("exercises")
                    .Doc(new
                    {
                        muscles = exerciseDoc.Muscles,
                        lastUpdated = updatedMuscle.LastUpdated
                    })
                    .DocAsUpsert(false)
            );

            if (!updateResponse.IsValid)
            {
                Console.WriteLine($"Error updating muscle in exercise {exerciseId}: {updateResponse.ServerError?.Error?.Reason ?? updateResponse.DebugInformation}");
            }
        }
    }
}