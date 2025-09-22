
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Test.Databases.Journal;

namespace Test.WeekPlans;

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
        var weekPlan = new Databases.App.Tables.WeekPlan.Table()
        {
            Id = Guid.NewGuid(),
            WorkoutId = Guid.NewGuid(),
            DateOfWeek = "Monday",
            Time = TimeSpan.Zero,
            CreatedDate = DateTime.UtcNow,
            LastUpdated = DateTime.UtcNow
        };
        dbContext.WeekPlans.Add(weekPlan);
        await dbContext.SaveChangesAsync();

        var weekPlansEndpoint = serviceProvider!.GetRequiredService<Library.WeekPlans.Interface>();
        var result = await weekPlansEndpoint.AllAsync(new()
        {
            PageIndex = 0,
            PageSize = 10
        });
        Assert.NotNull(result);
        Assert.NotNull(result.Data);
        Assert.NotNull(result.Data.Items);
        Assert.True(result.Data.Items.Count > 0, "Expected at least one week plan in result.");

        dbContext.WeekPlans.Remove(weekPlan);
        await dbContext.SaveChangesAsync();
    }

    [Fact]

    public async Task POST()
    {
        var id = Guid.NewGuid();
        var workoutId = Guid.NewGuid();
        var dateOfWeek = $"Monday{id}";
        var dbContext = serviceProvider!.GetRequiredService<JournalDbContext>();
        dbContext.WeekPlans.RemoveRange(dbContext.WeekPlans.
            Where(x => x.DateOfWeek == dateOfWeek));

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


        var payload = new Library.WeekPlans.Create.Payload()
        {
            WorkoutId = workoutId,
            DateOfWeek = dateOfWeek,
            Time = TimeSpan.Zero,
        };
        var weekPlansEndpoint = serviceProvider!.GetRequiredService<Library.WeekPlans.Interface>();
        await weekPlansEndpoint.CreateAsync(payload);

        var expected = await dbContext.WeekPlans.FirstOrDefaultAsync(x => x.DateOfWeek == dateOfWeek);
        Assert.NotNull(expected);
        Assert.True(expected.WorkoutId == workoutId);
        Assert.Equal(expected.DateOfWeek, payload.DateOfWeek);
        Assert.Equal(expected.Time, payload.Time);

        dbContext.Workouts.Remove(existingWorkout);
        dbContext.WeekPlans.Remove(expected);
        await dbContext.SaveChangesAsync();
    }

    [Fact]

    public async Task PUT()
    {
        var id = Guid.NewGuid();
        var updatedWorkoutId = Guid.NewGuid();
        var dbContext = serviceProvider!.GetRequiredService<JournalDbContext>();
        var existingWeekPlan = new Databases.App.Tables.WeekPlan.Table()
        {
            Id = id,
            WorkoutId = Guid.NewGuid(),
            DateOfWeek = "Monday",
            Time = TimeSpan.Zero
        };
        dbContext.WeekPlans.Add(existingWeekPlan);
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

        var weekPlansEndpoint = serviceProvider!.GetRequiredService<Library.WeekPlans.Interface>();
        var updatedDateOfWeek = "Tuesday";
        var payload = new Library.WeekPlans.Update.Payload
        {
            Id = id,
            WorkoutId = updatedWorkoutId,
            DateOfWeek = updatedDateOfWeek,
            Time = TimeSpan.Zero
        };
        await weekPlansEndpoint.UpdateAsync(payload);

        await dbContext.Entry(existingWeekPlan).ReloadAsync();
        var updatedWeekPlan = existingWeekPlan;
        Assert.NotNull(updatedWeekPlan);
        Assert.Equal(updatedWeekPlan.WorkoutId, updatedWorkoutId);
        Assert.Equal(updatedWeekPlan.DateOfWeek, updatedDateOfWeek);
        Assert.Equal(updatedWeekPlan.Time, payload.Time);

        dbContext.Workouts.Remove(existingWorkout);
        dbContext.WeekPlans.Remove(updatedWeekPlan);
        await dbContext.SaveChangesAsync();
    }

    [Fact]
    public async Task DELETE()
    {
        var id = Guid.NewGuid();
        var dbContext = serviceProvider!.GetRequiredService<JournalDbContext>();
        var weekPlan = new Databases.App.Tables.WeekPlan.Table()
        {
            Id = id,
            WorkoutId = Guid.NewGuid(),
            DateOfWeek = "Monday",
            Time = TimeSpan.Zero
        };
        dbContext.WeekPlans.Add(weekPlan);
        await dbContext.SaveChangesAsync();

        var weekPlansEndpoint = serviceProvider!.GetRequiredService<Library.WeekPlans.Interface>();
        await weekPlansEndpoint.DeleteAsync(new() {Id = id});

        await dbContext.Entry(weekPlan).ReloadAsync();
        var deletedWeekPlan = await dbContext.WeekPlans.FindAsync(id);
        Assert.Null(deletedWeekPlan);
    }
    #endregion
}
