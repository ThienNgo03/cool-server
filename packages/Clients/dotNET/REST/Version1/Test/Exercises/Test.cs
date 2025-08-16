
using Library;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
using System.Text.Json;
using Test.Constant;
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
        string? token = GetBearerToken();
        if(string.IsNullOrEmpty(token))
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
    public async Task GET_PushUpExist()
    {
        var dbContext = serviceProvider!.GetRequiredService<JournalDbContext>();
        var id = Guid.NewGuid();
        var pushUp = new Databases.Journal.Tables.Exercise.Table()
        {
            Id = id,
            Name = "Push Up",
            Description = "A basic exercise for upper body strength.",
            MusclesWorked = "Chest, Triceps, Shoulders",
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
        Guid Id = Guid.NewGuid();
        string pushUp = $"Push Up {Id}";
        string description = "A basic exercise for upper body strength.";
        string musclesWorked = "Chest, Triceps, Shoulders";
        var dbContext = serviceProvider!.GetRequiredService<JournalDbContext>();
        dbContext.Exercises.RemoveRange(
            dbContext.Exercises.Where(e => e.Name == pushUp && e.Description == description).ToList());
        await dbContext.SaveChangesAsync();
        var exercisesEndpoint = serviceProvider!.GetRequiredService<Library.Exercises.Interface>();

        var payload = new Library.Exercises.Create.Payload
        {
            Name = pushUp,
            Description = description,
            MusclesWorked = musclesWorked
        };
        await exercisesEndpoint.CreateAsync(payload);

        var expected = await dbContext.Exercises
            .FirstOrDefaultAsync(e => e.Name == pushUp);
        Assert.NotNull(expected);
        Assert.Equal(pushUp, expected.Name);
        Assert.Equal(description, expected.Description);
        Assert.Equal(musclesWorked, expected.MusclesWorked);

        Assert.True(expected.CreatedDate > DateTime.MinValue);
    }

    [Fact]

    public async Task PUT()
    {
        string pushUp = "Push Up";
        string description = "A basic exercise for upper body strength.";
        string musclesWorked = "Chest, Triceps, Shoulders";
        var dbContext = serviceProvider!.GetRequiredService<JournalDbContext>();
        var id = Guid.NewGuid();
        var existingExercise = new Databases.Journal.Tables.Exercise.Table
        {
            Id = id,
            Name = pushUp,
            Description = description,
            MusclesWorked = musclesWorked,
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
            Description = "An updated description for the push up exercise.",
            MusclesWorked = "Back, Biceps"
        };
        await exercisesEndpoint.UpdateAsync(payload);

        await dbContext.Entry(existingExercise).ReloadAsync();
        var updatedExercise = existingExercise;

        Assert.NotNull(updatedExercise);
        Assert.Equal("Pull Up", updatedExercise.Name);
        Assert.Equal("An updated description for the push up exercise.", updatedExercise.Description);
        Assert.Equal("Back, Biceps", updatedExercise.MusclesWorked);

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
        var existingExercise = new Databases.Journal.Tables.Exercise.Table
        {
            Id = id,
            Name = pushUp,
            Description = description,
            MusclesWorked = "Chest, Triceps, Shoulders",
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
