
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Test.Databases.App;

namespace Test.TeamPools;

public class Test : BaseTest
{

    #region [ CTors ]

    public Test() : base() { }
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
        var teamPool = new Databases.App.Tables.TeamPool.Table()
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
        var competition = new Databases.App.Tables.Competition.Table()
        {
            Id = competitionId,
            Title = "Test Competition",
            CreatedDate = DateTime.UtcNow
        };
        dbContext.Competitions.Add(competition);
        var teamPool = new Databases.App.Tables.TeamPool.Table()
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
        var teamPool = new Databases.App.Tables.TeamPool.Table()
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
}
