using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Test.Databases.App;

namespace Test.WeekPlanSets;

public class Test : BaseTest
{
    public Test() : base()
    {
    }

    [Fact]

    public async Task GET()
    {
        var dbContext = serviceProvider.GetRequiredService<JournalDbContext>();
        var id = Guid.NewGuid();
        var weekPlanSet = new Databases.App.Tables.WeekPlanSet.Table()
        {
            Id = id,
            CreatedDate = DateTime.UtcNow,
            InsertedBy = Guid.NewGuid(),
            LastUpdated = null,
            UpdatedBy = Guid.NewGuid(),
            WeekPlanId = Guid.NewGuid(),
            Value = 1
        };
        dbContext.WeekPlanSets.Add(weekPlanSet);
        await dbContext.SaveChangesAsync();
        var weekPlanSetsEndpoint = serviceProvider.GetRequiredService<Library.WeekPlanSets.Interface>();
        var response = await weekPlanSetsEndpoint.AllAsync(new Library.WeekPlanSets.All.Parameters()
        {
            PageIndex = 0,
            PageSize = 10
        });
        Assert.NotNull(response);
        Assert.NotNull(response.Data);
        Assert.NotNull(response.Data.Items);
        Assert.True(response.Data.Items.Any(x => x.Id == id), "Find the week plan set just added");

        dbContext.WeekPlanSets.Remove(weekPlanSet);
        await dbContext.SaveChangesAsync();
    }

    [Fact]

    public async Task POST()
    {
        var dbContext = serviceProvider.GetRequiredService<JournalDbContext>();
        var id = Guid.NewGuid();
        var weekPlanId = Guid.NewGuid();
        var value = 10;
        var weekPlan = new Databases.App.Tables.WeekPlan.Table()
        {
            Id = weekPlanId,
            WorkoutId = Guid.NewGuid(),
            DateOfWeek = "Monday",
            Time = TimeSpan.Zero,
            CreatedDate = DateTime.UtcNow,
            LastUpdated = DateTime.UtcNow
        };
        dbContext.WeekPlans.Add(weekPlan);
        await dbContext.SaveChangesAsync();

        var weekPlanSetsEndpoint = serviceProvider.GetRequiredService<Library.WeekPlanSets.Interface>();
        var payLoad = new Library.WeekPlanSets.Create.Payload()
        {
            WeekPlanId = weekPlanId,
            Value = value
        };
        await weekPlanSetsEndpoint.CreateAsync(payLoad);

        var expected = await dbContext.WeekPlanSets.FirstOrDefaultAsync(x => x.WeekPlanId == weekPlanId);
        Assert.NotNull(expected);
        Assert.Equal(expected.WeekPlanId, weekPlanId);
        Assert.Equal(expected.Value, value);

        dbContext.WeekPlans.Remove(weekPlan);
        dbContext.WeekPlanSets.Remove(expected);
        await dbContext.SaveChangesAsync();
    }

    [Fact]

    public async Task PUT()
    {
        var dbContext = serviceProvider.GetRequiredService<JournalDbContext>();
        var id = Guid.NewGuid();
        var updatedWeekPlanId = Guid.NewGuid();
        var updatedValue = 10;
        var weekPlan = new Databases.App.Tables.WeekPlan.Table()
        {
            Id =updatedWeekPlanId,
            WorkoutId = Guid.NewGuid(),
            DateOfWeek = "Monday",
            Time = TimeSpan.Zero,
            CreatedDate = DateTime.UtcNow,
            LastUpdated = DateTime.UtcNow
        };
        dbContext.WeekPlans.Add(weekPlan);

        var weekPlanSet = new Databases.App.Tables.WeekPlanSet.Table()
        {
            Id = id,
            WeekPlanId = Guid.NewGuid(),
            Value = 12,
            LastUpdated = null,
            CreatedDate = DateTime.UtcNow,
            InsertedBy = Guid.NewGuid(),
            UpdatedBy = Guid.NewGuid()
        };
        dbContext.WeekPlanSets.Add(weekPlanSet);
        await dbContext.SaveChangesAsync();

        var weekPlanSetsEndpoint = serviceProvider.GetRequiredService<Library.WeekPlanSets.Interface>();
        var payload = new Library.WeekPlanSets.Update.Payload()
        {
            Id = id,
            WeekPlanId = updatedWeekPlanId,
            Value = updatedValue
        };
        await weekPlanSetsEndpoint.UpdateAsync(payload);

        await dbContext.Entry(weekPlanSet).ReloadAsync();

        var updatedWeekPlanSet = weekPlanSet;

        Assert.NotNull(updatedWeekPlanSet);
        Assert.Equal(updatedWeekPlanSet.WeekPlanId, updatedWeekPlanId);
        Assert.Equal(updatedWeekPlanSet.Value, updatedValue);

        dbContext.WeekPlanSets.Remove(updatedWeekPlanSet);
        dbContext.WeekPlans.Remove(weekPlan);
        await dbContext.SaveChangesAsync();

    }

    [Fact]

    public async Task Delete()
    {
        var id = Guid.NewGuid();
        var dbContext = serviceProvider.GetRequiredService<JournalDbContext>();
        var weekPlanSet = new Databases.App.Tables.WeekPlanSet.Table()
        {
            Id = id,
            WeekPlanId = Guid.NewGuid(),
            Value = 12,
            LastUpdated = null,
            CreatedDate = DateTime.UtcNow,
            InsertedBy = Guid.NewGuid(),
            UpdatedBy = Guid.NewGuid()
        };
        dbContext.WeekPlanSets.Add(weekPlanSet);
        await dbContext.SaveChangesAsync();

        var weekPlanSetsEndpoint = serviceProvider.GetRequiredService<Library.WeekPlanSets.Interface>();
        var parameters = new Library.WeekPlanSets.Delete.Parameters() { Id = id };
        await weekPlanSetsEndpoint.DeleteAsync(parameters);

        await dbContext.Entry(weekPlanSet).ReloadAsync();
        var deletedWeekPlanSet = await dbContext.WeekPlanSets.FindAsync(id);
        Assert.Null(deletedWeekPlanSet);
    }
}
