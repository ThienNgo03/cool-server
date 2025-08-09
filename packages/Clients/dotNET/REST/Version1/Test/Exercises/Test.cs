
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
        var exercisesEndpoint = serviceProvider!.GetRequiredService<Library.Exercises.Interface>();
        var payload = new Library.Exercises.Create.Payload
        {
            Name = "Push Up",
            Description = "A basic exercise for upper body strength.",
        };
        await exercisesEndpoint.CreateAsync(payload);
         //Verify that the exercise was created
    }
    #endregion
}
