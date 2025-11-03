namespace Journal.ExerciseMuscles.Post.Messager;

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
        // Get the muscle information from database
        var muscle = await _context.Muscles.FirstOrDefaultAsync(x => x.Id == message.exerciseMuscles.MuscleId);

        if (muscle == null)
        {
            Console.WriteLine($"Muscle with ID {message.exerciseMuscles.MuscleId} not found.");
            return;
        }

        // Get the current exercise document from OpenSearch
        var getResponse = await _openSearchClient.GetAsync<Databases.OpenSearch.Indexes.Exercise.Index>(
            message.exerciseMuscles.ExerciseId.ToString(),
            g => g.Index("exercises")
        );

        if (!getResponse.IsValid)
        {
            Console.WriteLine($"Error retrieving exercise from OpenSearch: {getResponse.ServerError?.Error?.Reason ?? getResponse.DebugInformation}");
            return;
        }

        var exerciseDoc = getResponse.Source;

        // Initialize muscles list if null
        if (exerciseDoc.Muscles == null)
        {
            exerciseDoc.Muscles = new List<Databases.OpenSearch.Indexes.Muscle.Index>();
        }

        // Create new muscle document
        var newMuscle = new Databases.OpenSearch.Indexes.Muscle.Index
        {
            Id = muscle.Id,
            Name = muscle.Name,
            CreatedDate = muscle.CreatedDate,
            LastUpdated = muscle.LastUpdated
        };

        if (!exerciseDoc.Muscles.Any(m => m.Id == newMuscle.Id))
        {
            exerciseDoc.Muscles.Add(newMuscle);

            var updateResponse = await _openSearchClient.UpdateAsync<Databases.OpenSearch.Indexes.Exercise.Index, object>(
                message.exerciseMuscles.ExerciseId.ToString(),
                u => u
                    .Index("exercises")
                    .Doc(new
                    {
                        muscles = exerciseDoc.Muscles,
                        lastUpdated = message.exerciseMuscles.CreatedDate
                    })
                    .DocAsUpsert(false)
            );

            if (!updateResponse.IsValid)
            {
                Console.WriteLine($"Error updating muscles in OpenSearch: {updateResponse.ServerError?.Error?.Reason ?? updateResponse.DebugInformation}");
            }
        }
        else
        {
            Console.WriteLine($"Muscle {newMuscle.Id} already exists in exercise {message.exerciseMuscles.ExerciseId}.");
        }
    }
}
