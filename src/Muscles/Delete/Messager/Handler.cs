namespace Journal.Muscles.Delete.Messager;

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

        // Remove the muscle from each exercise document
        foreach (var exerciseId in exerciseIds)
        {
            await RemoveMuscleFromExercise(exerciseId, message.muscleId);
        }
    }

    private async Task RemoveMuscleFromExercise(Guid exerciseId, Guid muscleId)
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

        // Remove the muscle from the list
        var initialCount = exerciseDoc.Muscles.Count;
        exerciseDoc.Muscles.RemoveAll(m => m.Id == muscleId);

        // Only update if something was removed
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