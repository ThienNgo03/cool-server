
using Test.Databases.Journal;
using Microsoft.EntityFrameworkCore;
using Library.Queryable.Include.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Library.Competitions.All;

namespace Test.Workouts;

public class Test : BaseTest
{

    #region [ CTors ]

    public Test() : base() { }
    #endregion

    #region [ Actions ]

    [Fact]
    public async Task All()
    {
        var dbContext = serviceProvider!.GetRequiredService<JournalDbContext>();
        var workout = new Databases.App.Tables.Workout.Table()
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
        var result = await workoutsEndpoint.AllAsync<Library.Workouts.All.Parameters>(new() 
        {
            IsIncludeWeekPlans = true,
            IsIncludeWeekPlanSets = true,
            IsIncludeExercises = true,
            IsIncludeMuscles = true,
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
    public async Task All_Includes()
    {
        var workoutService = serviceProvider!.GetRequiredService<Library.Workouts.Interface>();
        try
        {
            var result = await workoutService
                .Include(x => x.Exercise)
                    .ThenInclude(x => x.Muscles)
                .AllAsync<Library.Workouts.All.Parameters>(new()
                {
                    PageIndex = 0,
                    PageSize = 5
                });

            var result1 = await workoutService
                .Include(x => x.WeekPlans)
                    .ThenInclude(x => x.WeekPlanSets)
                .AllAsync<Library.Workouts.All.Parameters>(new()
                {
                    PageIndex = 0,
                    PageSize = 5
                });


            var result2 = await workoutService
                .Include(x => x.Exercise)
                    .ThenInclude(x => x.Muscles)
                .Include(x => x.WeekPlans)
                    .ThenInclude(x => x.WeekPlanSets)
                .AllAsync<Library.Workouts.All.Parameters>(new()
                {
                    PageIndex = 0,
                    PageSize = 5
                });

            Assert.NotNull(result);
            Assert.NotNull(result1);
            Assert.NotNull(result2);
        }
        catch (Exception ex)
        {
            // If test fails, provide useful error message
            Assert.Fail($"Fluent Include API failed: {ex.Message}");
        }
    }

    [Fact]
    public async Task Add()
    {
        var exerciseId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var dbContext = serviceProvider!.GetRequiredService<JournalDbContext>();
        dbContext.Workouts.RemoveRange(dbContext.Workouts.
            Where(x => x.ExerciseId == exerciseId && x.UserId == userId));
        var existingExercise = new Databases.App.Tables.Exercise.Table()
        {
            Id = exerciseId,
            Name = "Push Up",
            Description = "A basic exercise for upper body strength.",
            Type = "Rep",
            CreatedDate = DateTime.UtcNow,
            LastUpdated = DateTime.UtcNow
        };
        dbContext.Exercises.Add(existingExercise);

        var existingUser = new Databases.App.Tables.User.Table()
        {
            Id = userId,
            Name = "Push Up",
            Email = "abc@gmail.com",
            PhoneNumber = "1234567890",
        };
        dbContext.Users.Add(existingUser);
        await dbContext.SaveChangesAsync();

        var workoutsEndpoint = serviceProvider!.GetRequiredService<Library.Workouts.Interface>();
        var payload = new Library.Workouts.Create.Payload
        {
            ExerciseId = exerciseId,
            UserId = userId,
        };
        await workoutsEndpoint.CreateAsync(payload);

        var expected = await dbContext.Workouts.FirstOrDefaultAsync(w => w.UserId == userId && w.ExerciseId == exerciseId);
        Assert.NotNull(expected);

        dbContext.Workouts.Remove(expected);
        dbContext.Exercises.Remove(existingExercise);
        dbContext.Users.Remove(existingUser);
        await dbContext.SaveChangesAsync();
    }

    [Fact]

    public async Task Update()
    {
        var id = Guid.NewGuid();
        var updatedExerciseId = Guid.NewGuid();
        var updatedUserId = Guid.NewGuid();
        var dbContext = serviceProvider!.GetRequiredService<JournalDbContext>();
        var existingWorkout = new Databases.App.Tables.Workout.Table()
        {
            Id = id,
            ExerciseId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            CreatedDate = DateTime.UtcNow,
            LastUpdated = DateTime.UtcNow
        };
        dbContext.Workouts.Add(existingWorkout);

        var existingExercise = new Databases.App.Tables.Exercise.Table()
        {
            Id = updatedExerciseId,
            Name = "Push Up",
            Description = "A basic exercise for upper body strength.",
            Type = "Rep",
            CreatedDate = DateTime.UtcNow,
            LastUpdated = DateTime.UtcNow
        };
        dbContext.Exercises.Add(existingExercise);

        var existingUser = new Databases.App.Tables.User.Table()
        {
            Id = updatedUserId,
            Name = "Push Up",
            Email = "abc@gmail.com",
            PhoneNumber = "1234567890",
        };
        dbContext.Users.Add(existingUser);

        await dbContext.SaveChangesAsync();

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

        dbContext.Exercises.Remove(existingExercise);
        dbContext.Users.Remove(existingUser);
        dbContext.Workouts.Remove(updatedWorkout);
        await dbContext.SaveChangesAsync();
    }

    [Fact]

    public async Task Delete()
    {
        var dbContext = serviceProvider!.GetRequiredService<JournalDbContext>();
        var id = Guid.NewGuid();
        var workoutToDelete = new Databases.App.Tables.Workout.Table()
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
