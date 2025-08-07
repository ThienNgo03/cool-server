
using Library;
using Microsoft.Extensions.DependencyInjection;

namespace Test.Exercises;

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
        serviceProvider = services.BuildServiceProvider();
    }
    #endregion

    #region [ Endpoints ]

    [Fact]
    public async Task GET_PushUpExist()
    {
        var exercisesEndpoint = serviceProvider!.GetRequiredService<Library.Exercises.Interface>();
        var result = await exercisesEndpoint.AllAsync(new() 
        {
            PageIndex = 0,
            PageSize = 10,
            Name = "Pull up"
        });

        Assert.NotNull(result);
        Assert.NotNull(result.Data);
        Assert.NotNull(result.Data.Items);
        Assert.True(result.Data.Items.Count > 0, "Expected at least one exercise in result.");
        Assert.True(result.Data.Items.Any(e => e.Name == "Push Up"), "Expected to find 'Push Up' exercise in the result.");
    }

    #endregion
}
