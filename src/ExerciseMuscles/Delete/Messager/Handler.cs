namespace Journal.ExerciseMuscles.Delete.Messager;

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
        var getResponse = await _openSearchClient.GetAsync<Databases.OpenSearch.Indexes.Exercise.Index>(
            message.exerciseId.ToString(),
            g => g.Index("exercises")
        );

        if (!getResponse.IsValid)
        {
            Console.WriteLine($"Error retrieving exercise {message.exerciseId} from OpenSearch: {getResponse.ServerError?.Error?.Reason ?? getResponse.DebugInformation}");
            return;
        }

        var exerciseDoc = getResponse.Source;

        if (exerciseDoc.Muscles == null || !exerciseDoc.Muscles.Any())
        {
            Console.WriteLine($"No muscles found for exercise {message.exerciseId}.");
            return;
        }

        // Remove the old muscle
        var muscleToRemove = exerciseDoc.Muscles.FirstOrDefault(m => m.Id == message.muscleId);

        if (muscleToRemove != null)
        {
            exerciseDoc.Muscles.Remove(muscleToRemove);

            // Update the document
            var updateResponse = await _openSearchClient.UpdateAsync<Databases.OpenSearch.Indexes.Exercise.Index, object>(
                message.exerciseId.ToString(),
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
                Console.WriteLine($"Error removing muscle from exercise in OpenSearch: {updateResponse.ServerError?.Error?.Reason ?? updateResponse.DebugInformation}");
            }
        }
        else
        {
            Console.WriteLine($"Muscle {message.muscleId} not found in exercise {message.exerciseId}.");
        }
    }
}