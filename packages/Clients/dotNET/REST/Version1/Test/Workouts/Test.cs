using Library;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Refit;
using Test.Constant;
using Test.Databases.Journal;

namespace Test.Workouts;

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
    public async Task GET()
    {
        var dbContext = serviceProvider!.GetRequiredService<JournalDbContext>();
        var workout = new Databases.Journal.Tables.Workout.Table()
        {
            Id = Guid.NewGuid(),
            ExerciseId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            CreatedDate = DateTime.UtcNow,
            LastUpdated = DateTime.UtcNow
        };
        dbContext.Workouts.Add(workout);
        await dbContext.SaveChangesAsync();

        var workoutsEndpoint = serviceProvider!.GetRequiredService<Library.Workouts.Interface>();
        var result = await workoutsEndpoint.AllAsync(new() 
        {
            PageIndex = 0,
            PageSize = 10
        });
        Assert.NotNull(result);
        Assert.NotNull(result.Data);
        Assert.NotNull(result.Data.Items);
        Assert.True(result.Data.Items.Count > 0, "Expected at least one workout in result.");

        dbContext.Workouts.Remove(workout); 
        await dbContext.SaveChangesAsync();
    }

    [Fact]
    public async Task POST()
    {
        var exerciseId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var dbContext = serviceProvider!.GetRequiredService<JournalDbContext>();
        dbContext.Workouts.RemoveRange(dbContext.Workouts.
            Where(x => x.ExerciseId == exerciseId && x.UserId == userId));
        await dbContext.SaveChangesAsync();

        var workoutsEndpoint = serviceProvider!.GetRequiredService<Library.Workouts.Interface>();
        var payload = new Library.Workouts.Create.Payload
        {
            ExerciseId = exerciseId,
            UserId = userId,
        };
        await workoutsEndpoint.CreateAsync(payload);

        var workout = await dbContext.Workouts.FirstOrDefaultAsync(w => w.UserId == userId && w.ExerciseId == exerciseId);
        Assert.NotNull(workout);

        dbContext.Workouts.Remove(workout);
        await dbContext.SaveChangesAsync();
    }

    [Fact]

    public async Task PUT()
    {
        var dbContext = serviceProvider!.GetRequiredService<JournalDbContext>();
        var id = Guid.NewGuid();
        var existingWorkout = new Databases.Journal.Tables.Workout.Table()
        {
            Id = id,
            ExerciseId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            CreatedDate = DateTime.UtcNow,
            LastUpdated = DateTime.UtcNow
        };
        dbContext.Workouts.Add(existingWorkout);
        await dbContext.SaveChangesAsync();

        var updatedExerciseId = Guid.NewGuid();
        var updatedUserId = Guid.NewGuid();
        var payload = new Library.Workouts.Update.Payload
        {
            Id = id,
            ExerciseId = updatedExerciseId,
            UserId = updatedUserId
        };
        var workoutsEndpoint = serviceProvider!.GetRequiredService<Library.Workouts.Interface>();
        await workoutsEndpoint.UpdateAsync(payload);

        await dbContext.Entry(existingWorkout).ReloadAsync();
        var updatedWorkout = existingWorkout;
        Assert.NotNull(updatedWorkout);
        Assert.Equal(updatedExerciseId, updatedWorkout.ExerciseId);
        Assert.Equal(updatedUserId, updatedWorkout.UserId);

        dbContext.Workouts.Remove(updatedWorkout);
        await dbContext.SaveChangesAsync();
    }

    [Fact]

    public async Task DELETE()
    {
        var dbContext = serviceProvider!.GetRequiredService<JournalDbContext>();
        var id = Guid.NewGuid();
        var workoutToDelete = new Databases.Journal.Tables.Workout.Table()
        {
            Id = id,
            ExerciseId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            CreatedDate = DateTime.UtcNow,
            LastUpdated = DateTime.UtcNow
        };
        dbContext.Workouts.Add(workoutToDelete);
        await dbContext.SaveChangesAsync();

        var workoutsEndpoint = serviceProvider!.GetRequiredService<Library.Workouts.Interface>();
        await workoutsEndpoint.DeleteAsync(new() { Id = id });

        await dbContext.Entry(workoutToDelete).ReloadAsync();
        var deletedWorkout = await dbContext.Workouts.FindAsync(id);
        Assert.Null(deletedWorkout);
    }
    #endregion
}
