namespace Journal.Muscles.Patch.Messager;

using Journal.Databases;
using Microsoft.EntityFrameworkCore;
using OpenSearch.Client;

public class Handler
{
    private readonly JournalDbContext _context;
    private readonly IOpenSearchClient _openSearchClient;

    public Handler(JournalDbContext context, IOpenSearchClient openSearchClient)
    {
        _context = context;
        _openSearchClient = openSearchClient;
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

        // Update the muscle in each exercise document
        foreach (var exerciseId in exerciseIds)
        {
            await PatchMuscleInExercise(exerciseId, updatedMuscle, message.changes);
        }
    }

    private async Task PatchMuscleInExercise(Guid exerciseId, Table updatedMuscle, List<(string Path, object? Value)> changes)
    {
        // Get current exercise document
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

        // Find the muscle to update
        var muscleToUpdate = exerciseDoc.Muscles.FirstOrDefault(m => m.Id == updatedMuscle.Id);

        if (muscleToUpdate != null)
        {
            // Apply changes to the muscle
            foreach (var (path, value) in changes)
            {
                var fieldName = path.TrimStart('/');

                switch (fieldName.ToLowerInvariant())
                {
                    case "name":
                        muscleToUpdate.Name = value?.ToString() ?? muscleToUpdate.Name;
                        break;
                        // Add other patchable fields as needed
                }
            }

            // Always update lastUpdated
            muscleToUpdate.LastUpdated = updatedMuscle.LastUpdated;

            // Update the document in OpenSearch
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