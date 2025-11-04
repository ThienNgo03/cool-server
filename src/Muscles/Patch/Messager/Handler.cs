namespace Journal.Muscles.Patch.Messager;

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

        // Get the updated muscle from database
        var updatedMuscle = await _context.Muscles.FirstOrDefaultAsync(m => m.Id == message.muscleId);
        if (updatedMuscle == null)
        {
            Console.WriteLine($"Muscle {message.muscleId} not found.");
            return;
        }

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
                await PatchMuscleInExerciseOpenSearch(exerciseId, updatedMuscle, message.changes);
            }
        }
        catch
        {
            Console.WriteLine("Can't reach OpenSearch");
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
                return;
            }

            int updatedCount = 0;
            foreach (var workout in workouts)
            {
                if (workout.Exercise?.Muscles == null || !workout.Exercise.Muscles.Any())
                    continue;

                var muscleToUpdate = workout.Exercise.Muscles.FirstOrDefault(m => m.Id == updatedMuscle.Id);
                if (muscleToUpdate != null)
                {
                    foreach (var (path, value) in message.changes)
                    {
                        var fieldName = path.TrimStart('/').ToLowerInvariant();
                        switch (fieldName)
                        {
                            case "name":
                                muscleToUpdate.Name = value?.ToString() ?? muscleToUpdate.Name;
                                break;
                                // Add other patchable fields as needed
                        }
                    }

                    muscleToUpdate.LastUpdated = updatedMuscle.LastUpdated;
                    workout.LastUpdated = DateTime.UtcNow;
                    updatedCount++;
                }
            }

            if (updatedCount > 0)
            {
                _mongoDbContext.Workouts.UpdateRange(workouts.Where(w =>
                    w.Exercise?.Muscles?.Any(m => m.Id == message.muscleId) == true));
                await _mongoDbContext.SaveChangesAsync();
                Console.WriteLine($"Patched muscle in {updatedCount} workout(s)");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"MongoDB error: {ex.Message}");
            throw;
        }
    }

    private async Task PatchMuscleInExerciseOpenSearch(Guid exerciseId, Table updatedMuscle, List<(string Path, object? Value)> changes)
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
            foreach (var (path, value) in changes)
            {
                var fieldName = path.TrimStart('/').ToLowerInvariant();
                switch (fieldName)
                {
                    case "name":
                        muscleToUpdate.Name = value?.ToString() ?? muscleToUpdate.Name;
                        break;
                        // Add other patchable fields as needed
                }
            }

            muscleToUpdate.LastUpdated = updatedMuscle.LastUpdated;

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
                Console.WriteLine($"Error patching muscle in exercise {exerciseId}: {updateResponse.ServerError?.Error?.Reason ?? updateResponse.DebugInformation}");
            }
        }
    }
}