
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Test.Databases.App;

namespace Test.WorkoutLogs;

public class Test : BaseTest
{

    #region [ CTors ]

    public Test() : base() { }
    #endregion

    #region [ Endpoints ]
    [Fact]
    public async Task GET()
    {
        var dbContext = serviceProvider!.GetRequiredService<JournalDbContext>();
        var workoutLog = new Databases.App.Tables.WorkoutLog.Table()
        {
            Id = Guid.NewGuid(),
            WorkoutId = Guid.NewGuid(),
            WorkoutDate = DateTime.UtcNow,
            CreatedDate = DateTime.UtcNow,
            LastUpdated = DateTime.UtcNow
        };
        dbContext.WorkoutLogs.Add(workoutLog);
        await dbContext.SaveChangesAsync();

        var workoutLogsEndpoint = serviceProvider!.GetRequiredService<Library.WorkoutLogs.Interface>();
        var result = await workoutLogsEndpoint.AllAsync(new()
        {
            PageIndex = 0,
            PageSize = 10
        });
        Assert.NotNull(result);
        Assert.NotNull(result.Data);
        Assert.NotNull(result.Data.Items);
        Assert.True(result.Data.Items.Count > 0, "Expected at least one week plan in result.");

        dbContext.WorkoutLogs.Remove(workoutLog);
        await dbContext.SaveChangesAsync();
    }

    [Fact]

    public async Task POST()
    {
        var workoutId = Guid.NewGuid();
        // Ensure no poluted data exists
        var dbContext = serviceProvider!.GetRequiredService<JournalDbContext>();
        dbContext.WorkoutLogs.RemoveRange(dbContext.WorkoutLogs.
            Where(x => x.WorkoutId == workoutId));
        var existingWorkout = new Databases.App.Tables.Workout.Table()
        {
            Id = workoutId,
            ExerciseId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            CreatedDate = DateTime.UtcNow,
            LastUpdated = DateTime.UtcNow
        };
        dbContext.Workouts.Add(existingWorkout);
        await dbContext.SaveChangesAsync();

        var payload = new Library.WorkoutLogs.Create.Payload()
        {
            WorkoutId = workoutId,
            WorkoutDate = DateTime.UtcNow,
        };
        var workoutLogsEndpoint = serviceProvider!.GetRequiredService<Library.WorkoutLogs.Interface>();
        await workoutLogsEndpoint.CreateAsync(payload);

        var expected = await dbContext.WorkoutLogs.FirstOrDefaultAsync(x => x.WorkoutId == workoutId);
        Assert.NotNull(expected);
        Assert.Equal(expected.WorkoutId, workoutId);
        Assert.Equal(expected.WorkoutDate, payload.WorkoutDate);

        dbContext.WorkoutLogs.Remove(expected);
        dbContext.Workouts.Remove(existingWorkout);
        await dbContext.SaveChangesAsync();
    }

    [Fact]

    public async Task PUT()
    {
        var id = Guid.NewGuid();
        var updatedWorkoutId = Guid.NewGuid();
        var dbContext = serviceProvider!.GetRequiredService<JournalDbContext>();
        var existingWorkoutLog = new Databases.App.Tables.WorkoutLog.Table()
        {
            Id = id,
            WorkoutId = Guid.NewGuid(),
            WorkoutDate = DateTime.UtcNow
        };
        dbContext.WorkoutLogs.Add(existingWorkoutLog);
        var existingWorkout = new Databases.App.Tables.Workout.Table()
        {
            Id = updatedWorkoutId,
            ExerciseId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            CreatedDate = DateTime.UtcNow,
            LastUpdated = DateTime.UtcNow
        };
        dbContext.Workouts.Add(existingWorkout);
        await dbContext.SaveChangesAsync();

        var workoutLogsEndpoint = serviceProvider!.GetRequiredService<Library.WorkoutLogs.Interface>();
        var payload = new Library.WorkoutLogs.Update.Payload
        {
            Id = id,
            WorkoutId = updatedWorkoutId,
            WorkoutDate = DateTime.UtcNow
        };
        await workoutLogsEndpoint.UpdateAsync(payload);

        await dbContext.Entry(existingWorkoutLog).ReloadAsync();
        var updatedWorkoutLog = existingWorkoutLog;
        Assert.NotNull(updatedWorkoutLog);
        Assert.Equal(updatedWorkoutLog.WorkoutId, updatedWorkoutId);
        Assert.Equal(updatedWorkoutLog.WorkoutDate.Date, payload.WorkoutDate.Date);

        dbContext.WorkoutLogs.Remove(updatedWorkoutLog);
        dbContext.Workouts.Remove(existingWorkout);
        await dbContext.SaveChangesAsync();
    }

    [Fact]
    public async Task DELETE()
    {
        var id = Guid.NewGuid();
        var dbContext = serviceProvider!.GetRequiredService<JournalDbContext>();
        var workoutLog = new Databases.App.Tables.WorkoutLog.Table()
        {
            Id = id,
            WorkoutId = Guid.NewGuid(),
            WorkoutDate = DateTime.UtcNow
        };
        dbContext.WorkoutLogs.Add(workoutLog);
        await dbContext.SaveChangesAsync();

        var workoutLogsEndpoint = serviceProvider!.GetRequiredService<Library.WorkoutLogs.Interface>();
        await workoutLogsEndpoint.DeleteAsync(new() { Id = id });

        await dbContext.Entry(workoutLog).ReloadAsync();
        var deletedWorkoutLog = await dbContext.WorkoutLogs.FindAsync(id);
        Assert.Null(deletedWorkoutLog);
    }
    #endregion
}
