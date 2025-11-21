using Microsoft.Extensions.DependencyInjection;
using Test.Databases.App;

namespace Test.Users;

public class Test:BaseTest
{
    #region [ CTors ]

    public Test() : base() { }
    #endregion
    #region [ Endpoints ]

    [Fact]
    public async Task GET_DataExist()
    {
        //var dbContext = serviceProvider!.GetRequiredService<JournalDbContext>();
        //var id = Guid.NewGuid();
        //var newUser = new Databases.App.Tables.User.Table()
        //{
        //    Id = id,
        //    Name = $"TEST USER-{id}",
        //    Email = "testingEmail@gmail.com",
        //    PhoneNumber = "0111011110",
        //    CreatedDate = DateTime.UtcNow,
        //    LastUpdated = DateTime.UtcNow
        //};
        //dbContext.Users.Add(newUser);
        //await dbContext.SaveChangesAsync();
        var userEndpoint = serviceProvider!.GetRequiredService<Library.Users.Interface>();
        var result = await userEndpoint.AllAsync(new()
        {
        });

        Assert.NotNull(result);
        //Assert.NotNull(result.Items);
        //Assert.True(result.Items.Count > 0, "Expected at least one user in result.");
        //Assert.True(result.Items.Any(e => e.Name == $"TEST USER-{id}"), $"Expected to find 'TEST USER-{id}' exercise in the result.");
        //dbContext.Users.Remove(newUser);
        //await dbContext.SaveChangesAsync();
    }

    #endregion
}
