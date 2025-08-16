
using Library;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
using System.Text.Json;
using Test.Constant;
using Test.Databases.Journal;

namespace Test.TeamPools;

public class Test
{
	#region [ Fields ] 

	private readonly IServiceProvider serviceProvider;

    #endregion

    #region [ CTors ]

    public Test()
    {
        string? token = GetBearerToken();
        if (string.IsNullOrEmpty(token))
            throw new InvalidOperationException("Failed to retrieve authentication token.");

        var services = new ServiceCollection();
        services.AddEndpoints(isLocal: true, token);

        services.AddDbContext<JournalDbContext>(options =>
           options.UseSqlServer(Config.ConnectionString));

        serviceProvider = services.BuildServiceProvider();
    }
    #endregion

    #region [ Endpoints ]

    [Fact]
    public async Task GET_CompetitionExist()
    {
        var dbContext = serviceProvider!.GetRequiredService<JournalDbContext>();
        var id = Guid.NewGuid();
        int position= 1;
        Guid participantId = Guid.NewGuid();
        string competitionId = "87619852-3344-4EC3-B2CB-051F29C5C1C1";
        Guid convertCompetitionId= Guid.Parse(competitionId);
        var teamPool = new Databases.Journal.Tables.TeamPool.Table()
        {
            Id = id,
            CompetitionId= convertCompetitionId,
            Position= position,
            ParticipantId = participantId,
            CreatedDate = DateTime.UtcNow,
            
        };
        dbContext.TeamPools.Add(teamPool);
        await dbContext.SaveChangesAsync();
        var teamPoolsEndpoint = serviceProvider!.GetRequiredService<Library.TeamPools.Interface>();
        var result = await teamPoolsEndpoint.AllAsync(new Library.TeamPools.All.Parameters
        {
            CompetitionId= convertCompetitionId,
            PageIndex = 0,
            PageSize = 10
        });

        Assert.NotNull(result);
        Assert.NotNull(result.Data);
        Assert.NotNull(result.Data.Items);
        Assert.True(result.Data.Items.Count > 0, "Expected at least one exercise in result.");
        Assert.True(result.Data.Items.Any(e => e.CompetitionId  == convertCompetitionId), $"Expected to find {competitionId} exercise in the result.");
        dbContext.TeamPools.Remove(teamPool);
        await dbContext.SaveChangesAsync();
    }


    [Fact]

    public async Task POST()
    {
        int position = 1;
        Guid participantId = Guid.NewGuid();
        Guid competitionId = Guid.NewGuid();
        var dbContext = serviceProvider!.GetRequiredService<JournalDbContext>();
        dbContext.TeamPools.RemoveRange(
            dbContext.TeamPools.Where(e => e.CompetitionId == competitionId).ToList());
        dbContext.Competitions.Add(new()
        {
            Id = competitionId,
            Title = "Test Competition",
            CreatedDate = DateTime.UtcNow
        });
        await dbContext.SaveChangesAsync();
        var teamPoolsEndpoint = serviceProvider!.GetRequiredService<Library.TeamPools.Interface>();

        var payload = new Library.TeamPools.Create.Payload
        {
            CompetitionId = competitionId,
            ParticipantId= participantId,
            Position = position
        };
        await teamPoolsEndpoint.CreateAsync(payload);

        var expected = await dbContext.TeamPools
            .FirstOrDefaultAsync(e => e.CompetitionId == competitionId);
        Assert.NotNull(expected);
        Assert.Equal(competitionId, expected.CompetitionId);
        Assert.Equal(participantId, expected.ParticipantId);
        Assert.Equal(position, expected.Position);
        Assert.True(expected.CreatedDate > DateTime.MinValue);
    }

    [Fact]

    public async Task PUT()
    {
        var dbContext = serviceProvider!.GetRequiredService<JournalDbContext>();
        var id = Guid.NewGuid();
        int position = 1;
        Guid participantId = Guid.NewGuid();
        Guid competitionId = Guid.NewGuid();
        var competition = new Databases.Journal.Tables.Competition.Table()
        {
            Id = competitionId,
            Title = "Test Competition",
            CreatedDate = DateTime.UtcNow
        };
        dbContext.Competitions.Add(competition);
        var teamPool = new Databases.Journal.Tables.TeamPool.Table()
        {
            Id = id,
            CompetitionId = competitionId,
            ParticipantId = participantId,
            Position = position,
            CreatedDate = DateTime.UtcNow,

        };
        dbContext.TeamPools.Add(teamPool);
        await dbContext.SaveChangesAsync();
        var teamPoolsEndpoint = serviceProvider!.GetRequiredService<Library.TeamPools.Interface>();
        int newPosition = 2;
        var payload = new Library.TeamPools.Update.Payload
        {
            Id = id,
            ParticipantId = participantId,
            Position = newPosition,
            CompetitionId = competitionId,    
        };
        await teamPoolsEndpoint.UpdateAsync(payload);

        await dbContext.Entry(teamPool).ReloadAsync();
        var updatedTeamPool = teamPool;

        Assert.NotNull(updatedTeamPool);
        Assert.Equal(newPosition, updatedTeamPool.Position);

        dbContext.TeamPools.Remove(updatedTeamPool);
        await dbContext.SaveChangesAsync();
    }

    [Fact]

    public async Task DELETE()
    {
        var dbContext = serviceProvider!.GetRequiredService<JournalDbContext>();
        var id = Guid.NewGuid();
        int position = 1;
        Guid participantId = Guid.NewGuid();
        string competitionId = "87619852-3344-4EC3-B2CB-051F29C5C1C1";
        Guid convertCompetitionId= Guid.Parse(competitionId);
        var teamPool = new Databases.Journal.Tables.TeamPool.Table()
        {
            Id = id,
            CompetitionId= convertCompetitionId,
            ParticipantId= participantId,
            Position = position,
            CreatedDate = DateTime.UtcNow,
            
        };
        dbContext.TeamPools.Add(teamPool);
        await dbContext.SaveChangesAsync();
        var teamPoolsEndpoint = serviceProvider!.GetRequiredService<Library.TeamPools.Interface>();
        await teamPoolsEndpoint.DeleteAsync(new Library.TeamPools.Delete.Parameters { Id = id });

        await dbContext.Entry(teamPool).ReloadAsync();
        var deletedExercise = await dbContext.TeamPools.FindAsync(teamPool.Id);

        Assert.Null(deletedExercise);
    }
    #endregion

    #region [ Authentication ]

    private string? GetBearerToken()
    {
        var client = new HttpClient();
        var request = new HttpRequestMessage(HttpMethod.Post, "https://localhost:7011/api/authentication/login");

        var jsonPayload = @"{
            ""accountEmail"": ""systemtester@journal.com"",
            ""password"": ""NewPassword@1""
        }";

        request.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        var response = client.Send(request);
        response.EnsureSuccessStatusCode();

        var responseBody = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

        using var document = JsonDocument.Parse(responseBody);
        var token = document.RootElement.GetProperty("token").GetString();

        return token;
    }
    #endregion
}
