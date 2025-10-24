namespace Journal.Exercises.Post.Messager;

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

        var exercises = await _context.Exercises.AsNoTracking().ToListAsync();
        var exercise = exercises.FirstOrDefault(e => e.Id == message.Id);

        if (exercise != null)
        {
            var bulkData = new List<object>
            {
                new { index = new { _index = "exercises", _id = exercise.Id } },
                new
                {
                    exercise.Id,
                    exercise.Name,
                    exercise.Description,
                    exercise.Type,
                    muscles = new List<object>(), // để trống lúc này
                    exercise.CreatedDate,
                    exercise.LastUpdated
                }
            };

            var bulkResponse = await client.BulkAsync<StringResponse>(PostData.MultiJson(bulkData));
            if (!bulkResponse.Success)
            {
                Console.WriteLine($"Error indexing document: {bulkResponse.DebugInformation}");
            }
        }
    }
}