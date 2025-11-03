namespace Journal.ExerciseMuscles.Patch.Messager;

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

        // Check if MuscleId was changed
        var muscleIdChange = message.changes.FirstOrDefault(c =>
            c.Path.TrimStart('/').Equals("MuscleId", StringComparison.OrdinalIgnoreCase));

        if (muscleIdChange.Path != null)
        {
            // MuscleId was changed - need to replace the muscle in the exercise
            var newMuscleId = (Guid)muscleIdChange.Value!;
            await ReplaceMuscleInExercise(message.entity.ExerciseId, message.entity.MuscleId, newMuscleId);
        }

        // Check if ExerciseId was changed
        var exerciseIdChange = message.changes.FirstOrDefault(c =>
            c.Path.TrimStart('/').Equals("ExerciseId", StringComparison.OrdinalIgnoreCase));

        if (exerciseIdChange.Path != null)
        {
            // ExerciseId was changed - move muscle to different exercise
            var newExerciseId = (Guid)exerciseIdChange.Value!;
            var oldMuscleId = message.entity.MuscleId;

            // If MuscleId was also changed, use the new one
            var muscleIdToAdd = muscleIdChange.Path != null
                ? (Guid)muscleIdChange.Value!
                : oldMuscleId;

            await RemoveMuscleFromExercise(message.entity.ExerciseId, oldMuscleId);
            await AddMuscleToExercise(newExerciseId, muscleIdToAdd);
        }
    }

    private async Task ReplaceMuscleInExercise(Guid exerciseId, Guid oldMuscleId, Guid newMuscleId)
    {
        // Get the new muscle info
        var newMuscle = await _context.Muscles.FirstOrDefaultAsync(x => x.Id == newMuscleId);

        if (newMuscle == null)
        {
            Console.WriteLine($"New muscle with ID {newMuscleId} not found.");
            return;
        }

        // Get current exercise document
        var getResponse = await _openSearchClient.GetAsync<Databases.OpenSearch.Indexes.Exercise.Index>(
            exerciseId.ToString(),
            g => g.Index("exercises")
        );

        if (!getResponse.IsValid)
        {
            Console.WriteLine($"Error retrieving exercise from OpenSearch: {getResponse.ServerError?.Error?.Reason ?? getResponse.DebugInformation}");
            return;
        }

        var exerciseDoc = getResponse.Source;

        if (exerciseDoc.Muscles == null)
        {
            exerciseDoc.Muscles = new List<Databases.OpenSearch.Indexes.Muscle.Index>();
        }

        // Remove old muscle
        exerciseDoc.Muscles.RemoveAll(m => m.Id == oldMuscleId);

        // Add new muscle if not already exists
        if (!exerciseDoc.Muscles.Any(m => m.Id == newMuscleId))
        {
            exerciseDoc.Muscles.Add(new Databases.OpenSearch.Indexes.Muscle.Index
            {
                Id = newMuscle.Id,
                Name = newMuscle.Name,
                CreatedDate = newMuscle.CreatedDate,
                LastUpdated = newMuscle.LastUpdated
            });
        }

        // Update the document
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
            Console.WriteLine($"Error replacing muscle in exercise: {updateResponse.ServerError?.Error?.Reason ?? updateResponse.DebugInformation}");
        }
    }

    private async Task RemoveMuscleFromExercise(Guid exerciseId, Guid muscleId)
    {
        var getResponse = await _openSearchClient.GetAsync<Databases.OpenSearch.Indexes.Exercise.Index>(
            exerciseId.ToString(),
            g => g.Index("exercises")
        );

        if (!getResponse.IsValid)
        {
            Console.WriteLine($"Error retrieving exercise: {getResponse.ServerError?.Error?.Reason ?? getResponse.DebugInformation}");
            return;
        }

        var exerciseDoc = getResponse.Source;

        if (exerciseDoc.Muscles != null)
        {
            exerciseDoc.Muscles.RemoveAll(m => m.Id == muscleId);

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
                Console.WriteLine($"Error removing muscle: {updateResponse.ServerError?.Error?.Reason ?? updateResponse.DebugInformation}");
            }
        }
    }

    private async Task AddMuscleToExercise(Guid exerciseId, Guid muscleId)
    {
        var muscle = await _context.Muscles.FirstOrDefaultAsync(x => x.Id == muscleId);

        if (muscle == null)
        {
            Console.WriteLine($"Muscle with ID {muscleId} not found.");
            return;
        }

        var getResponse = await _openSearchClient.GetAsync<Databases.OpenSearch.Indexes.Exercise.Index>(
            exerciseId.ToString(),
            g => g.Index("exercises")
        );

        if (!getResponse.IsValid)
        {
            Console.WriteLine($"Error retrieving exercise: {getResponse.ServerError?.Error?.Reason ?? getResponse.DebugInformation}");
            return;
        }

        var exerciseDoc = getResponse.Source;

        if (exerciseDoc.Muscles == null)
        {
            exerciseDoc.Muscles = new List<Databases.OpenSearch.Indexes.Muscle.Index>();
        }

        if (!exerciseDoc.Muscles.Any(m => m.Id == muscleId))
        {
            exerciseDoc.Muscles.Add(new Databases.OpenSearch.Indexes.Muscle.Index
            {
                Id = muscle.Id,
                Name = muscle.Name,
                CreatedDate = muscle.CreatedDate,
                LastUpdated = muscle.LastUpdated
            });

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
                Console.WriteLine($"Error adding muscle: {updateResponse.ServerError?.Error?.Reason ?? updateResponse.DebugInformation}");
            }
        }
    }
}