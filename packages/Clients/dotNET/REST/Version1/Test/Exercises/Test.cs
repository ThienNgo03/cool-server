using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Test.Databases.App;

namespace Test.Exercises;

public class Test : BaseTest
{

    #region [ CTors ]

    public Test() : base() { }
    #endregion

    #region [ Endpoints ]

    [Fact]
    public async Task GET_DataExist()
    {
        var dbContext = serviceProvider!.GetRequiredService<JournalDbContext>();
        var id = Guid.NewGuid();
        var pushUp = new Databases.App.Tables.Exercise.Table()
        {
            Id = id,
            Name = $"TEST EXERCISE-{id}",
            Description = "A basic TEST EXERCISE for upper body strength.",
            Type = "Rep",
            CreatedDate = DateTime.UtcNow,
            LastUpdated = DateTime.UtcNow
        };
        dbContext.Exercises.Add(pushUp);
        await dbContext.SaveChangesAsync();
        var exercisesEndpoint = serviceProvider!.GetRequiredService<Library.Exercises.Interface>();
        var result = await exercisesEndpoint.AllAsync(new() 
        {
        });

        Assert.NotNull(result);
        Assert.NotNull(result);
        Assert.NotNull(result.Items);
        Assert.True(result.Items.Count > 0, "Expected at least one exercise in result.");
        Assert.True(result.Items.Any(e => e.Name == $"TEST EXERCISE-{id}"), "Expected to find 'TEST EXERCISE' exercise in the result.");
        dbContext.Exercises.Remove(pushUp);
        await dbContext.SaveChangesAsync();
    }

    [Fact]
    public async Task ALL_Included()
    {
        
        var exercisesEndpoint = serviceProvider!.GetRequiredService<Library.Exercises.Interface>();
        var result = await exercisesEndpoint.AllAsync(new()
        {
            PageIndex = 0,
            PageSize = 10,
            Include = "muscles",
        });

        Assert.NotNull(result);
        Assert.NotNull(result);
        Assert.NotNull(result.Items);
        Assert.True(result.Items.Count > 0, "Expected at least one exercise in result.");
        var firstExercise = result.Items.First();
        Assert.NotNull(firstExercise.Muscles);
    }

    [Fact]

    public async Task POST()
    {
        Guid Id = Guid.NewGuid();
        string testExercise = $"TEST EXERCISE {Id}";
        string description = "A basic TEST EXERCISE for upper body strength.";
        string type = "Rep";
        var dbContext = serviceProvider!.GetRequiredService<JournalDbContext>();
        dbContext.Exercises.RemoveRange(
            dbContext.Exercises.Where(e => e.Name == testExercise && e.Description == description).ToList());
        await dbContext.SaveChangesAsync();
        var exercisesEndpoint = serviceProvider!.GetRequiredService<Library.Exercises.Interface>();

        var payload = new Library.Exercises.POST.Payload
        {
            Name = testExercise,
            Description = description,
            Type = type
        };
        await exercisesEndpoint.CreateAsync(payload);

        var expected = await dbContext.Exercises
            .FirstOrDefaultAsync(e => e.Name == testExercise);
        Assert.NotNull(expected);
        Assert.Equal(testExercise, expected.Name);
        Assert.Equal(description, expected.Description);
        Assert.Equal(type, expected.Type);

        Assert.True(expected.CreatedDate > DateTime.MinValue);
        
        dbContext.Exercises.Remove(expected);
        await dbContext.SaveChangesAsync();
    }

    [Fact]

    public async Task PUT()
    {
        string pushUp = "TEST EXERCISE";
        string description = "A basic TEST EXERCISE for upper body strength.";
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
        var payload = new Library.Exercises.PUT.Payload
        {
            Id = id,
            Name = "UPDATED TEST EXERCISE",
            Type = "Rep",
            Description = "An updated TEST EXERCISE for the push up exercise."
        };
        await exercisesEndpoint.UpdateAsync(payload);

        await dbContext.Entry(existingExercise).ReloadAsync();
        var updatedExercise = existingExercise;

        Assert.NotNull(updatedExercise);
        Assert.Equal("UPDATED TEST EXERCISE", updatedExercise.Name);
        Assert.Equal("An updated TEST EXERCISE for the push up exercise.", updatedExercise.Description);
        Assert.Equal("Rep", updatedExercise.Type);

        dbContext.Exercises.Remove(updatedExercise);
        await dbContext.SaveChangesAsync();
    }

    [Fact]

    public async Task DELETE()
    {
        string pushUp = "TEST EXERCISE";
        string description = "A basic TEST EXERCISE for upper body strength.";
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
        await exercisesEndpoint.DeleteAsync(new Library.Exercises.DELETE.Parameters { Id = id });

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
        Assert.NotNull(result.Items);
        Assert.True(result.Items.Count > 0, "Expected at least one muscle in result.");
    }


}
