using Library.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Test.Exercises;

public class Test
{
    [Fact]
    public void ToList()
    {
        var serviceProvider = Config.ConfigureServices();
        using var scope = serviceProvider.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<JournalContext>();

        //dbContext.Database.Add(exercies1);

        var exercises = context.Exercises
                            .UseQueryStyle(Library.Queryable.QueryStyle.Rest)
                            //.Where(x => x.Name == "Pull up")
                            .Take(10).Skip(0)
                            .ToList();

        var competitions = context.Competitions
                            .UseQueryStyle(Library.Queryable.QueryStyle.Rest)
                            .Take(10).Skip(0).ToList();

        var gadgets = context.Gadgets
                            .UseQueryStyle(Library.Queryable.QueryStyle.Rest)
                            .Take(10).Skip(0).ToList();

        //context.Exercises.AddAsync(new() { Name = "Cool", Description = "Nice exe" });
    }

    [Fact]
    public void Add()
    {
        var serviceProvider = Config.ConfigureServices();
        using var scope = serviceProvider.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<JournalContext>();

        var exercise = new Model
        {
            Name = "Pull up",
            Description = "Pull up exercise"
        };

        context.Exercises.AddAsync(exercise);

        //EF Core
        //dbContext.Database.Exercies.FirstOrDefault(x => x.Name == "Pull up");
        //Asert.NotNull(exercise);
        //Assert.Equal("Pull up", exercise.Name);
    }
}