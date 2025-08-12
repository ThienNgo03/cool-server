using Library;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Test.Constant;
using Test.Databases.Journal;

namespace Test.Competitions;

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
    public async Task GET_PushUpTitleExist()
    {
        var dbContext = serviceProvider!.GetRequiredService<JournalDbContext>();
        var id = Guid.NewGuid();
        var participantId1 = Guid.NewGuid();
        var participantId2 = Guid.NewGuid();
        var exerciseId = Guid.NewGuid();
        List<Guid> participantIds = new() { participantId1, participantId2 };
        var pushUpChalenge = new Databases.Journal.Tables.Competition.Table()
        {
            Id = id,
            Title = "Push Up",
            Description = "A basic exercise for upper body strength.",
            ParticipantIds = participantIds,
            CreatedDate = DateTime.UtcNow,
            ExerciseId = exerciseId,
            Location = "Gym",
            DateTime = DateTime.UtcNow.AddDays(7),
            Type = "Solo"
        };
        dbContext.Competitions.Add(pushUpChalenge);
        await dbContext.SaveChangesAsync();
        var competitionEndpoint = serviceProvider!.GetRequiredService<Library.Competitions.Interface>();
        var result = await competitionEndpoint.AllAsync(new() 
        {
            PageIndex = 1,
            PageSize = 1,
            Title = "Push Up"
        });

        Assert.NotNull(result);
        Assert.NotNull(result.Data);
        Assert.NotNull(result.Data.Items);
        Assert.True(result.Data.Items.Count > 0, "Expected at least one exercise in result.");
        Assert.True(result.Data.Items.Any(e => e.Title == "Push Up"), "Expected to find 'Push Up' exercise in the result.");
        dbContext.Competitions.Remove(pushUpChalenge);
        await dbContext.SaveChangesAsync();
    }


    [Fact]

    public async Task POST()
    {
        Guid Id = Guid.NewGuid();
        string pushUp = $"Push Up {Id}";
        string description = "A basic exercise for upper body strength.";
        var participantId1 = Guid.NewGuid();
        var participantId2 = Guid.NewGuid();
        var exerciseId = Guid.NewGuid();
        var dbContext = serviceProvider!.GetRequiredService<JournalDbContext>();
        dbContext.Exercises.RemoveRange(
            dbContext.Exercises.Where(e => e.Name == pushUp && e.Description == description).ToList());
        await dbContext.SaveChangesAsync();
        var competitionEndpoint = serviceProvider!.GetRequiredService<Library.Competitions.Interface>();

        var payload = new Library.Competitions.Create.Payload
        {
            Title = pushUp,
            Description = description,
            Location = "Gym",
            ExerciseId = exerciseId,
            DateTime = DateTime.UtcNow.AddDays(7),
            Type = "Solo"
        };
        await competitionEndpoint.CreateAsync(payload);

        var expected = await dbContext.Competitions
            .FirstOrDefaultAsync(e => e.Title == pushUp);
        Assert.NotNull(expected);
        Assert.Equal(pushUp, expected.Title);
        Assert.Equal(description, expected.Description);
        Assert.True(expected.CreatedDate > DateTime.MinValue);
    }

    [Fact]

    public async Task PUT()
    {
        string pushUp = "Push Up";
        string description = "A basic exercise for upper body strength.";
        var dbContext = serviceProvider!.GetRequiredService<JournalDbContext>();
        var id = Guid.NewGuid();
        var exerciseId = Guid.NewGuid();
        var existingCompetition = new Databases.Journal.Tables.Competition.Table
        {
            Id = id,
            Title = pushUp,
            Description = description,
            Location = "Gym",
            ExerciseId = exerciseId,
            DateTime = DateTime.UtcNow.AddDays(7),
            Type = "Solo"
        };
        dbContext.Competitions.Add(existingCompetition);
        await dbContext.SaveChangesAsync();
        var competitionEndpoint = serviceProvider!.GetRequiredService<Library.Competitions.Interface>();
        var payload = new Library.Competitions.Update.Payload
        {
            Id = id,
            Title = "Updated Push Up",
            Description = "An updated description for the push up exercise.",
            Location = "Gym",
            ExerciseId = exerciseId,
            DateTime = DateTime.UtcNow.AddDays(7),
            Type = "Solo"
        };
        await competitionEndpoint.UpdateAsync(payload);

        await dbContext.Entry(existingCompetition).ReloadAsync();
        var updatedCompetition = existingCompetition;

        Assert.NotNull(updatedCompetition);
        Assert.Equal("Updated Push Up", updatedCompetition.Title);
        Assert.Equal("An updated description for the push up exercise.", updatedCompetition.Description);

        dbContext.Competitions.Remove(updatedCompetition);
        await dbContext.SaveChangesAsync();
    }

    [Fact]

    public async Task DELETE()
    {
        string pushUp = "Push Up";
        string description = "A basic exercise for upper body strength.";
        var dbContext = serviceProvider!.GetRequiredService<JournalDbContext>();
        var id = Guid.NewGuid();
        var exerciseId = Guid.NewGuid();
        var existingCompetition = new Databases.Journal.Tables.Competition.Table
        {
            Id = id,
            Title = pushUp,
            Description = description,
            Location = "Gym",
            ExerciseId = exerciseId,
            DateTime = DateTime.UtcNow.AddDays(7),
            Type = "Solo"
        };
        dbContext.Competitions.Add(existingCompetition);
        await dbContext.SaveChangesAsync();
        var exercisesEndpoint = serviceProvider!.GetRequiredService<Library.Competitions.Interface>();
        await exercisesEndpoint.DeleteAsync(new Library.Competitions.Delete.Parameters { Id = id });

        await dbContext.Entry(existingCompetition).ReloadAsync();
        var deletedExercise = await dbContext.Competitions.FindAsync(existingCompetition.Id);

        Assert.Null(deletedExercise);
    }
    #endregion
}
