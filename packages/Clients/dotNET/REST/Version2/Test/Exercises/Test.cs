using Library.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Test.Exercises;

public class Test
{
    [Fact]
    public void Test1()
    {
        var serviceProvider = Config.ConfigureServices();
        using var scope = serviceProvider.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<JournalContext>();

        var exercises = context.Exercises
                            .UseQueryStyle(Library.Queryable.QueryStyle.Rest)
                            //.Where(x => x.Name == "Pull up")
                            //.Take(10).Skip(0)
                            .ToList();

        //var competitions = context.Competitions
        //                    .UseQueryStyle(Library.Queryable.QueryStyle.Rest)
        //                    .Take(10).Skip(0).ToList();

        //context.Exercises.AddAsync(new() { Name = "Cool", Description = "Nice exe" });
    }
}