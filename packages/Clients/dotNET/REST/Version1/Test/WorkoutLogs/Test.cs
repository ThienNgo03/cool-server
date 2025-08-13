using Library;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Refit;
using System;
using Test.Constant;
using Test.Databases.Journal;

namespace Test.WorkoutLogs;

public class Test
{
    #region [Fields]

    private readonly IServiceProvider serviceProvider;

    #endregion

    #region [CTors]
    public Test()
    {
        var services = new ServiceCollection();
        services.AddEndpoints(isLocal: true);

        services.AddDbContext<JournalDbContext>(options =>
           options.UseSqlServer(Config.ConnectionString));

        serviceProvider = services.BuildServiceProvider();
    }
    #endregion

    #region [Endpoints]
    [Fact]
    public async Task GET()
    {
        var dbContext = serviceProvider!.GetRequiredService<JournalDbContext>();
        var workoutLog = new Databases.Journal.Tables.WorkoutLog.Table()
        {
            Id = Guid.NewGuid(),
            WorkoutId = Guid.NewGuid(),
            Rep = 10,
            Set = 3,
            HoldingTime = TimeSpan.FromSeconds(30),
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
        var id = Guid.NewGuid();

        // Ensure no poluted data exists
        var dbContext = serviceProvider!.GetRequiredService<JournalDbContext>();
        dbContext.WorkoutLogs.RemoveRange(dbContext.WorkoutLogs.
            Where(x => x.WorkoutId == id));
        await dbContext.SaveChangesAsync();

        var payload = new Library.WorkoutLogs.Create.Payload()
        {
            WorkoutId = id,
            Rep = 10,
            Set = 3,
            HoldingTime = TimeSpan.FromSeconds(30),
            WorkoutDate = DateTime.UtcNow,
        };
        var workoutLogsEndpoint = serviceProvider!.GetRequiredService<Library.WorkoutLogs.Interface>();
        await workoutLogsEndpoint.CreateAsync(payload);

        var expected = await dbContext.WorkoutLogs.FirstOrDefaultAsync(x => x.WorkoutId == id);
        Assert.NotNull(expected);

        dbContext.WorkoutLogs.Remove(expected);
        await dbContext.SaveChangesAsync();
    }

    [Fact]

    public async Task PUT()
    {
        var id = Guid.NewGuid();
        var dbContext = serviceProvider!.GetRequiredService<JournalDbContext>();
        var existingWorkoutLog = new Databases.Journal.Tables.WorkoutLog.Table()
        {
            Id = id,
            WorkoutId = Guid.NewGuid(),
            Rep = 10,
            Set = 3,
            HoldingTime = TimeSpan.FromSeconds(30),
            WorkoutDate = DateTime.UtcNow
        };
        dbContext.WorkoutLogs.Add(existingWorkoutLog);
        await dbContext.SaveChangesAsync();

        var workoutLogsEndpoint = serviceProvider!.GetRequiredService<Library.WorkoutLogs.Interface>();
        var updatedRep = 12;
        var updatedSet = 2;
        var updatedHoldingTime = TimeSpan.FromSeconds(20);
        var payload = new Library.WorkoutLogs.Update.Payload
        {
            Id = id,
            WorkoutId = Guid.NewGuid(),
            Rep = updatedRep,
            Set = updatedSet,
            HoldingTime = updatedHoldingTime,
            WorkoutDate = DateTime.UtcNow
        };
        await workoutLogsEndpoint.UpdateAsync(payload);

        await dbContext.Entry(existingWorkoutLog).ReloadAsync();
        var updatedWorkoutLog = existingWorkoutLog;
        Assert.NotNull(updatedWorkoutLog);
        Assert.Equal(updatedWorkoutLog.Rep, updatedRep);
        Assert.Equal(updatedWorkoutLog.Set, updatedSet);
        Assert.Equal(updatedWorkoutLog.HoldingTime, updatedHoldingTime);

        dbContext.WorkoutLogs.Remove(updatedWorkoutLog);
        await dbContext.SaveChangesAsync();
    }

    [Fact]
    public async Task DELETE()
    {
        var id = Guid.NewGuid();
        var dbContext = serviceProvider!.GetRequiredService<JournalDbContext>();
        var workoutLog = new Databases.Journal.Tables.WorkoutLog.Table()
        {
            Id = id,
            WorkoutId = Guid.NewGuid(),
            Rep = 10,
            Set = 3,
            HoldingTime = TimeSpan.FromSeconds(30),
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
