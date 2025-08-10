
using Library;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Test.Databases.Journal;

namespace Test.Exercises;

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
           options.UseSqlServer("Server=localhost;Database=JOURNAL;Trusted_Connection=True;TrustServerCertificate=True;"));

        serviceProvider = services.BuildServiceProvider();
    }
    #endregion

    #region [ Endpoints ]

    [Fact]
    public async Task GET_PushUpExist()
    {
        var dbContext = serviceProvider!.GetRequiredService<JournalDbContext>();
        var id = Guid.NewGuid();
        var pushUp = new Databases.Journal.Tables.Excercise.Table()
        {
            Id = id,
            Name = "Push Up",
            Description = "A basic exercise for upper body strength.",
            CreatedDate = DateTime.UtcNow,
            LastUpdated = DateTime.UtcNow
        };
        dbContext.Exercises.Add(pushUp);
        await dbContext.SaveChangesAsync();
        var exercisesEndpoint = serviceProvider!.GetRequiredService<Library.Exercises.Interface>();
        var result = await exercisesEndpoint.AllAsync(new() 
        {
            PageIndex = 0,
            PageSize = 10,
            Name = "Push up"
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
        var dbContext = serviceProvider!.GetRequiredService<JournalDbContext>();
        // Ensure the database is clean before the test
        string pushUp="Push Up ver 1";
        dbContext.Exercises.RemoveRange(dbContext.Exercises.Where(x=>x.Name==pushUp));
        await dbContext.SaveChangesAsync();
        // Create a new exercise
        var exercisesEndpoint = serviceProvider!.GetRequiredService<Library.Exercises.Interface>();
        var payload = new Library.Exercises.Create.Payload
        {
            Name = "Push Up ver 1",
            Description = "A basic exercise for upper body strength.",
        };
        await exercisesEndpoint.CreateAsync(payload);
         //Verify that the exercise was created
         var createdExercise = await dbContext.Exercises.FirstOrDefaultAsync(e => e.Name == "Push Up ver 1");
        Guid exerciseId = createdExercise?.Id ?? Guid.Empty;
        Assert.NotNull(createdExercise);
        Assert.Equal("Push Up ver 1", createdExercise.Name);
        Assert.Equal("A basic exercise for upper body strength.", createdExercise.Description);
        //Clean up the created exercise
        dbContext.Exercises.Remove(createdExercise);
        await dbContext.SaveChangesAsync();
    }
    #endregion
}
