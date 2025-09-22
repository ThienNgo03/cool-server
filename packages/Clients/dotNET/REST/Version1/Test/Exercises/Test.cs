using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Test.Databases.Journal;

namespace Test.Exercises;

public class Test : BaseTest
{

    #region [ CTors ]

    public Test() : base() { }
    #endregion

    #region [ Endpoints ]

    [Fact]
    public async Task GET_PushUpExist()
    {
        var dbContext = serviceProvider!.GetRequiredService<JournalDbContext>();
        var id = Guid.NewGuid();
        var pushUp = new Databases.App.Tables.Exercise.Table()
        {
            Id = id,
            Name = "Push Up",
            Description = "A basic exercise for upper body strength.",
            Type = "Rep",
            CreatedDate = DateTime.UtcNow,
            LastUpdated = DateTime.UtcNow
        };
        dbContext.Exercises.Add(pushUp);
        await dbContext.SaveChangesAsync();
        var exercisesEndpoint = serviceProvider!.GetRequiredService<Library.Exercises.Interface>();
        var result = await exercisesEndpoint.AllAsync(new() 
        {
            PageIndex = 0,
            PageSize = 10
        });

        Assert.NotNull(result);
        Assert.NotNull(result.Data);
        Assert.NotNull(result.Data.Items);
        Assert.True(result.Data.Items.Count > 0, "Expected at least one exercise in result.");
        Assert.True(result.Data.Items.Any(e => e.Name == "Push Up"), "Expected to find 'Push Up' exercise in the result.");
        dbContext.Exercises.Remove(pushUp);
        await dbContext.SaveChangesAsync();
    }


    [Fact]

    public async Task POST()
    {
        Guid Id = Guid.NewGuid();
        string pushUp = $"Push Up {Id}";
        string description = "A basic exercise for upper body strength.";
        string type = "Rep";
        var dbContext = serviceProvider!.GetRequiredService<JournalDbContext>();
        dbContext.Exercises.RemoveRange(
            dbContext.Exercises.Where(e => e.Name == pushUp && e.Description == description).ToList());
        await dbContext.SaveChangesAsync();
        var exercisesEndpoint = serviceProvider!.GetRequiredService<Library.Exercises.Interface>();

        var payload = new Library.Exercises.Create.Payload
        {
            Name = pushUp,
            Description = description,
            Type = type
        };
        await exercisesEndpoint.CreateAsync(payload);

        var expected = await dbContext.Exercises
            .FirstOrDefaultAsync(e => e.Name == pushUp);
        Assert.NotNull(expected);
        Assert.Equal(pushUp, expected.Name);
        Assert.Equal(description, expected.Description);
        Assert.Equal(type, expected.Type);

        Assert.True(expected.CreatedDate > DateTime.MinValue);
    }

    [Fact]

    public async Task PUT()
    {
        string pushUp = "Push Up";
        string description = "A basic exercise for upper body strength.";
        string type = "rep";
        var dbContext = serviceProvider!.GetRequiredService<JournalDbContext>();
        var id = Guid.NewGuid();
        var existingExercise = new Databases.App.Tables.Exercise.Table
        {
            Id = id,
            Name = pushUp,
            Description = description,
            Type = type,
            CreatedDate = DateTime.UtcNow,
            LastUpdated = DateTime.UtcNow
        };
        dbContext.Exercises.Add(existingExercise);
        await dbContext.SaveChangesAsync();
        var exercisesEndpoint = serviceProvider!.GetRequiredService<Library.Exercises.Interface>();
        var payload = new Library.Exercises.Update.Payload
        {
            Id = id,
            Name = "Pull Up",
            Type = "Rep",
            Description = "An updated description for the push up exercise."
        };
        await exercisesEndpoint.UpdateAsync(payload);

        await dbContext.Entry(existingExercise).ReloadAsync();
        var updatedExercise = existingExercise;

        Assert.NotNull(updatedExercise);
        Assert.Equal("Pull Up", updatedExercise.Name);
        Assert.Equal("An updated description for the push up exercise.", updatedExercise.Description);
        Assert.Equal("Rep", updatedExercise.Type);

        dbContext.Exercises.Remove(updatedExercise);
        await dbContext.SaveChangesAsync();
    }

    [Fact]

    public async Task DELETE()
    {
        string pushUp = "Push Up";
        string description = "A basic exercise for upper body strength.";
        var dbContext = serviceProvider!.GetRequiredService<JournalDbContext>();
        var id = Guid.NewGuid();
        var existingExercise = new Databases.App.Tables.Exercise.Table
        {
            Id = id,
            Name = pushUp,
            Description = description,
            Type = "Rep",
            CreatedDate = DateTime.UtcNow,
            LastUpdated = DateTime.UtcNow
        };
        dbContext.Exercises.Add(existingExercise);
        await dbContext.SaveChangesAsync();
        var exercisesEndpoint = serviceProvider!.GetRequiredService<Library.Exercises.Interface>();
        await exercisesEndpoint.DeleteAsync(new Library.Exercises.Delete.Parameters { Id = id });

        await dbContext.Entry(existingExercise).ReloadAsync();
        var deletedExercise = await dbContext.Exercises.FindAsync(existingExercise.Id);

        Assert.Null(deletedExercise);
    }
    #endregion

    [Fact]
    public async Task Get_Muscles()
    {
        var exercisesEndpoint = serviceProvider!.GetRequiredService<Library.Muscles.Interface>();
        var result = await exercisesEndpoint.AllAsync(new()
        { 
        });

        Assert.NotNull(result);
        Assert.NotNull(result.Data);
        Assert.NotNull(result.Data.Items);
        Assert.True(result.Data.Items.Count > 0, "Expected at least one muscle in result.");
    }


}
