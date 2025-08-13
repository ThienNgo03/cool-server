
using Library;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Test.Constant;
using Test.Databases.Journal;

namespace Test.SoloPools;

public class Test
{
	#region [ Fields ] 

	private readonly IServiceProvider serviceProvider;

    #endregion

    #region [ CTors ]

    public Test()
    {
        var services = new ServiceCollection();
        services.AddEndpoints(isLocal: true);

        services.AddDbContext<JournalDbContext>(options =>
           options.UseSqlServer(Config.ConnectionString));

        serviceProvider = services.BuildServiceProvider();
    }
    #endregion

    #region [ Endpoints ]

    [Fact]
    public async Task GET_CompetitionExist()
    {
        #region [ Prepare ]

        var dbContext = serviceProvider!.GetRequiredService<JournalDbContext>();
        var id = Guid.NewGuid();
        Guid winnerId = Guid.NewGuid();
        Guid loserId = Guid.NewGuid();
        string competitionId = "87619852-3344-4EC3-B2CB-051F29C5C1C1";
        Guid convertCompetitionId = Guid.Parse(competitionId);
        var soloPool = new Databases.Journal.Tables.SoloPool.Table()
        {
            Id = id,
            CompetitionId = convertCompetitionId,
            WinnerId = winnerId,
            LoserId = loserId,
            CreatedDate = DateTime.UtcNow,

        };
        dbContext.SoloPools.Add(soloPool);
        await dbContext.SaveChangesAsync();
        #endregion

        #region [ Test ]

        var soloPoolsEndpoint = serviceProvider!.GetRequiredService<Library.SoloPools.Interface>();
        var result = await soloPoolsEndpoint.AllAsync(new Library.SoloPools.All.Parameters
        {
            CompetitionId = convertCompetitionId,
            PageIndex = 0,
            PageSize = 10
        });
        #endregion

        #region [ Check ]

        Assert.NotNull(result);
        Assert.NotNull(result.Data);
        Assert.NotNull(result.Data.Items);
        Assert.True(result.Data.Items.Count > 0, "Expected at least one exercise in result.");
        Assert.True(result.Data.Items.Any(e => e.CompetitionId == convertCompetitionId), $"Expected to find {competitionId} exercise in the result.");
        #endregion

        #region [ Clean up ]

        dbContext.SoloPools.Remove(soloPool);
        await dbContext.SaveChangesAsync();
        #endregion

    }


    [Fact]

    public async Task POST()
    {
        #region [ Prepare ]

        Guid winnerId = Guid.NewGuid();
        Guid loserId = Guid.NewGuid();
        Guid competitionId = Guid.NewGuid();
        var dbContext = serviceProvider!.GetRequiredService<JournalDbContext>();
        dbContext.SoloPools.RemoveRange(
            dbContext.SoloPools.Where(e => e.CompetitionId == competitionId).ToList());

        dbContext.Competitions.Add(new()
        {
            Id = competitionId,
            Title = "Test Competition",
            CreatedDate = DateTime.UtcNow
        });

        await dbContext.SaveChangesAsync();
        #endregion

        #region [ Tests ]

        var soloPoolsEndpoint = serviceProvider!.GetRequiredService<Library.SoloPools.Interface>();

        var payload = new Library.SoloPools.Create.Payload
        {
            CompetitionId = competitionId,
            WinnerId = winnerId,
            LoserId = loserId
        };
        await soloPoolsEndpoint.CreateAsync(payload);
        #endregion

        #region [ Check ]

        var expected = await dbContext.SoloPools
            .FirstOrDefaultAsync(e => e.CompetitionId == competitionId);
        Assert.NotNull(expected);
        Assert.Equal(competitionId, expected.CompetitionId);
        Assert.Equal(winnerId, expected.WinnerId);
        Assert.Equal(loserId, expected.LoserId);
        Assert.True(expected.CreatedDate > DateTime.MinValue);
        #endregion

        #region [ Clean up ]

        dbContext.SoloPools.Remove(expected);
        dbContext.Competitions.RemoveRange(
            dbContext.Competitions.Where(e => e.Id == competitionId).ToList());
        await dbContext.SaveChangesAsync();
        #endregion

    }

    [Fact]

    public async Task PUT()
    {
        var dbContext = serviceProvider!.GetRequiredService<JournalDbContext>();
        var id = Guid.NewGuid();
        Guid winnerId = Guid.NewGuid();
        Guid loserId = Guid.NewGuid();
        string competitionId = "87619852-3344-4EC3-B2CB-051F29C5C1C1";
        Guid convertCompetitionId = Guid.Parse(competitionId);
        string newCompetitionId = "2E1D1F19-0CEC-4FAC-997C-05DF512E2039";
        Guid convertNewCompetitionId = Guid.Parse(newCompetitionId);
        var soloPool = new Databases.Journal.Tables.SoloPool.Table()
        {
            Id = id,
            CompetitionId = convertCompetitionId,
            WinnerId = winnerId,
            LoserId = loserId,
            CreatedDate = DateTime.UtcNow,

        };
        dbContext.SoloPools.Add(soloPool);
        await dbContext.SaveChangesAsync();
        var soloPoolsEndpoint = serviceProvider!.GetRequiredService<Library.SoloPools.Interface>();
        var payload = new Library.SoloPools.Update.Payload
        {
            Id = id,
            CompetitionId = convertNewCompetitionId,    
        };
        await soloPoolsEndpoint.UpdateAsync(payload);

        await dbContext.Entry(soloPool).ReloadAsync();
        var updatedSoloPool = soloPool;

        Assert.NotNull(updatedSoloPool);
        Assert.Equal(convertNewCompetitionId, updatedSoloPool.CompetitionId);

        dbContext.SoloPools.Remove(updatedSoloPool);
        await dbContext.SaveChangesAsync();
    }

    [Fact]

    public async Task DELETE()
    {
        var dbContext = serviceProvider!.GetRequiredService<JournalDbContext>();
        var id = Guid.NewGuid();
        Guid winnerId= Guid.NewGuid();
        Guid loserId = Guid.NewGuid();
        string competitionId = "87619852-3344-4EC3-B2CB-051F29C5C1C1";
        Guid convertCompetitionId= Guid.Parse(competitionId);
        var soloPool = new Databases.Journal.Tables.SoloPool.Table()
        {
            Id = id,
            CompetitionId= convertCompetitionId,
            WinnerId = winnerId,
            LoserId = loserId,
            CreatedDate = DateTime.UtcNow,
            
        };
        dbContext.SoloPools.Add(soloPool);
        await dbContext.SaveChangesAsync();
        var soloPoolsEndpoint = serviceProvider!.GetRequiredService<Library.SoloPools.Interface>();
        await soloPoolsEndpoint.DeleteAsync(new Library.SoloPools.Delete.Parameters { Id = id });

        await dbContext.Entry(soloPool).ReloadAsync();
        var deletedExercise = await dbContext.Exercises.FindAsync(soloPool.Id);

        Assert.Null(deletedExercise);
    }
    #endregion
}
