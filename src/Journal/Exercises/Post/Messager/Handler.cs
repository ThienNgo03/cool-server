namespace Journal.Exercises.Post.Messager;
using Journal.Databases.MongoDb;
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
        if (message.exercise == null)
            return;

        // ===== SYNC OPENSEARCH =====
        try
        {
            var openSearchDocument = new Databases.OpenSearch.Indexes.Exercise.Index
            {
                Id = message.exercise.Id,
                Name = message.exercise.Name,
                Description = message.exercise.Description,
                Type = message.exercise.Type,
                Muscles = new List<Databases.OpenSearch.Indexes.Muscle.Index>(), // Empty at creation
                CreatedDate = message.exercise.CreatedDate,
                LastUpdated = message.exercise.LastUpdated
            };

            var indexResponse = await _openSearchClient.IndexAsync(openSearchDocument, i => i
                .Index("exercises")
                .Id(message.exercise.Id.ToString())
            );

            if (!indexResponse.IsValid)
            {
                Console.WriteLine($"Error indexing document: {indexResponse.ServerError?.Error?.Reason ?? indexResponse.DebugInformation}");
            }
        }
        catch
        {
            Console.WriteLine($"Can't reach OpenSearch");
        }

        // ===== SYNC MONGODB =====
        try
        {
            var mongoDocument = new Journal.Databases.MongoDb.Collections.Exercise.Collection
            {
                Id = message.exercise.Id,
                Name = message.exercise.Name,
                Description = message.exercise.Description,
                Type = message.exercise.Type,
                Muscles = new List<Journal.Databases.MongoDb.Collections.Exercise.Muscle>(), // Empty at creation
                CreatedDate = message.exercise.CreatedDate,
                LastUpdated = message.exercise.LastUpdated
            };

            _mongoDbContext.Exercises.Add(mongoDocument);
            await _mongoDbContext.SaveChangesAsync();

            Console.WriteLine($"Created exercise {message.exercise.Id} in MongoDB");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"MongoDB error: {ex.Message}");
            throw;
        }

        // ===== SYNC CONTEXT TABLES =====
        // No additional tables to sync for post operation
    }
}