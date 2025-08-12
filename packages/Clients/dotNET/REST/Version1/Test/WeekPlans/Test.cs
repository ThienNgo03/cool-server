using Library;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Refit;
using System;
using Test.Constant;
using Test.Databases.Journal;

namespace Test.WeekPlans;

public class Test
{
    #region [Fields]

    private readonly IServiceProvider serviceProvider;

    #endregion

    #region [CTors]
    public Test ()
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
        var weekPlan = new Databases.Journal.Tables.WeekPlan.Table()
        {
            Id = Guid.NewGuid(),
            WorkoutId = Guid.NewGuid(),
            Rep = 10,
            Set = 3,
            HoldingTime = TimeSpan.FromSeconds(30),
            DateOfWeek = "Monday",
            Time = DateTime.UtcNow,
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
        var dateOfWeek = $"Monday{id}";
        var dbContext = serviceProvider!.GetRequiredService<JournalDbContext>();
        dbContext.WeekPlans.RemoveRange(dbContext.WeekPlans.
            Where(x => x.DateOfWeek == dateOfWeek));

        var payload = new Library.WeekPlans.Create.Payload()
        {
            WorkoutId = Guid.NewGuid(),
            Rep = 10,
            Set = 3,
            HoldingTime = TimeSpan.FromSeconds(30),
            DateOfWeek = dateOfWeek,
            Time = DateTime.UtcNow
        };
        var weekPlansEndpoint = serviceProvider!.GetRequiredService<Library.WeekPlans.Interface>();
        await weekPlansEndpoint.CreateAsync(payload);

        var expected = await dbContext.WeekPlans.FirstOrDefaultAsync(x => x.DateOfWeek == dateOfWeek);
        Assert.NotNull(expected);
        Assert.True(expected.WorkoutId == payload.WorkoutId);
        
        dbContext.WeekPlans.Remove(expected);
        await dbContext.SaveChangesAsync();
    }

    [Fact]

    public async Task PUT()
    {
        var id = Guid.NewGuid();
        var dbContext = serviceProvider!.GetRequiredService<JournalDbContext>();
        var existingWeekPlan = new Databases.Journal.Tables.WeekPlan.Table()
        {
            Id = id,
            WorkoutId = Guid.NewGuid(),
            Rep = 10,
            Set = 3,
            HoldingTime = TimeSpan.FromSeconds(30),
            DateOfWeek = "Monday",
            Time = DateTime.UtcNow
        };
        dbContext.WeekPlans.Add(existingWeekPlan);
        await dbContext.SaveChangesAsync();

        var weekPlansEndpoint = serviceProvider!.GetRequiredService<Library.WeekPlans.Interface>();
        var updatedRep = 12;
        var updatedSet = 2;
        var updatedDateOfWeek = "Tuesday";
        var updatedHoldingTime = TimeSpan.FromSeconds(20);
        var payload = new Library.WeekPlans.Update.Payload
        {
            Id = id,
            WorkoutId = Guid.NewGuid(),
            Rep = updatedRep,
            Set = updatedSet,
            HoldingTime = updatedHoldingTime,
            DateOfWeek = updatedDateOfWeek,
            Time = DateTime.UtcNow
        };
        await weekPlansEndpoint.UpdateAsync(payload);

        await dbContext.Entry(existingWeekPlan).ReloadAsync();
        var updatedWeekPlan = existingWeekPlan;
        Assert.NotNull(updatedWeekPlan);
        Assert.Equal(updatedWeekPlan.Rep, updatedRep);
        Assert.Equal(updatedWeekPlan.DateOfWeek, updatedDateOfWeek);
        Assert.Equal(updatedWeekPlan.Set, updatedSet);
        Assert.Equal(updatedWeekPlan.HoldingTime, updatedHoldingTime);

        dbContext.WeekPlans.Remove(updatedWeekPlan);
        await dbContext.SaveChangesAsync();
    }

    [Fact]
    public async Task DELETE()
    {
        var id = Guid.NewGuid();
        var dbContext = serviceProvider!.GetRequiredService<JournalDbContext>();
        var weekPlan = new Databases.Journal.Tables.WeekPlan.Table()
        {
            Id = id,
            WorkoutId = Guid.NewGuid(),
            Rep = 10,
            Set = 3,
            HoldingTime = TimeSpan.FromSeconds(30),
            DateOfWeek = "Monday",
            Time = DateTime.UtcNow
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
