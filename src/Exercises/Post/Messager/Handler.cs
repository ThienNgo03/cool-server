namespace Journal.Exercises.Post.Messager;

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
        if (message.exercise != null)
        {
            var document = new Databases.OpenSearch.Indexes.Exercise.Index
            {
                Id = message.exercise.Id,
                Name = message.exercise.Name,
                Description = message.exercise.Description,
                Type = message.exercise.Type,
                Muscles = new List<Databases.OpenSearch.Indexes.Muscle.Index>(), // Empty at creation
                CreatedDate = message.exercise.CreatedDate,
                LastUpdated = message.exercise.LastUpdated
            };

            var indexResponse = await _openSearchClient.IndexAsync(document, i => i
                .Index("exercises")
                .Id(message.exercise.Id.ToString())
            );

            if (!indexResponse.IsValid)
            {
                Console.WriteLine($"Error indexing document: {indexResponse.ServerError?.Error?.Reason ?? indexResponse.DebugInformation}");
            }
        }
    }
}