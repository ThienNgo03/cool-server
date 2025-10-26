namespace Journal.Exercises.Delete.Messager;
using Microsoft.Extensions.Options;
using OpenSearch.Net;
using Journal.Databases;
using Journal.Databases.OpenSearch;

public class Handler
{
    private readonly JournalDbContext _context;
    private readonly OpenSearchConfig _config;

    public Handler(JournalDbContext context, IOptions<OpenSearchConfig> config)
    {
        _context = context;
        _config = config.Value;
    }

    public async Task Handle(Message message)
    {
        var builder = new ConnectionStringBuilder()
            .WithHost(_config.Host)
            .WithPort(_config.Port)
            .WithUsername(_config.Username)
            .WithPassword(_config.Password);

        if (_config.EnableSsl)
            builder.WithSsl();

        if (_config.SkipCertificateValidation)
            builder.WithSkipCertificateValidation();

        var uri = new Uri(builder.Build());
        var pool = new SingleNodeConnectionPool(uri);
        var settings = new ConnectionConfiguration(pool)
            .BasicAuthentication(_config.Username, _config.Password);

        if (_config.SkipCertificateValidation)
        {
            settings = settings.ServerCertificateValidationCallback((o, cert, chain, errors) => true);
        }

        var client = new OpenSearchLowLevelClient(settings);

        var response = await client.DeleteAsync<StringResponse>("exercises", message.Id.ToString());

        if (!response.Success)
        {
            Console.WriteLine($"Error deleting document from OpenSearch: {response.DebugInformation}");
        }

        var exerciseMuscles = _context.ExerciseMuscles.Where(em => em.ExerciseId == message.Id);
        _context.ExerciseMuscles.RemoveRange(exerciseMuscles);
        await _context.SaveChangesAsync();
    }
}
