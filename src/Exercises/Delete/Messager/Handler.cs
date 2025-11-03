namespace Journal.Exercises.Delete.Messager;

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
        // Delete from OpenSearch
        var deleteResponse = await _openSearchClient.DeleteAsync<Databases.OpenSearch.Indexes.Exercise.Index>(
            message.Id.ToString(),
            d => d.Index("exercises")
        );

        if (!deleteResponse.IsValid)
        {
            Console.WriteLine($"Error deleting document from OpenSearch: {deleteResponse.ServerError?.Error?.Reason ?? deleteResponse.DebugInformation}");
        }

        // Delete exercise muscles from database
        var exerciseMuscles = _context.ExerciseMuscles.Where(em => em.ExerciseId == message.Id);
        _context.ExerciseMuscles.RemoveRange(exerciseMuscles);
        await _context.SaveChangesAsync();
    }
}