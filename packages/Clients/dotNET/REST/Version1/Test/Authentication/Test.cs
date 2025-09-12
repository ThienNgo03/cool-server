
namespace Test.Authentication;

public class Test 
{
    #region [ CTors ]
    
    public Test()
    {
       
    }
    #endregion

    //#region [ Endpoints ]

    //[Fact]
    //public async Task POST_SignIn_StandaloneTest_ReturnsToken()
    //{
    //    var services = new ServiceCollection();
    //    var config = new Library.Config("https://localhost:7011");

    //    services.RegisterAuthentication(config);
    //    var provider = services.BuildServiceProvider();

    //    var authClient = provider.GetRequiredService<Library.Authentication.Interface>();

    //    var payload = new Library.Authentication.Signin.Payload
    //    {
    //        Account = "systemtester@journal.com",
    //        Password = "NewPassword@1"
    //    };

    //    var result = await authClient.SignInAsync(payload);

    //    Assert.NotNull(result);
    //    Assert.False(string.IsNullOrWhiteSpace(result.Token));
    //}

    //[Fact]
    //public async Task POST_SignIn_StandaloneTest_InvalidPassword()
    //{
    //    var services = new ServiceCollection();
    //    var config = new Library.Config("https://localhost:7011");

    //    services.RegisterAuthentication(config);
    //    var provider = services.BuildServiceProvider();

    //    var authClient = provider.GetRequiredService<Library.Authentication.Interface>();

    //    var payload = new Library.Authentication.Signin.Payload
    //    {
    //        Account = "systemtester@journal.com",
    //        Password = "NewPassword@3"
    //    };

    //    var ex = await Assert.ThrowsAsync<RpcException>(() => authClient.SignInAsync(payload));

    //    Assert.Equal(StatusCode.Unauthenticated, ex.StatusCode); 
    //    Assert.Contains("Invalid email or password", ex.Status.Detail, StringComparison.OrdinalIgnoreCase);
    //}
    //#endregion

    //[Fact]
    //public async Task POST_Register_ValidPayload_Succeeds()
    //{
    //    // Arrange
    //    var services = new ServiceCollection();
    //    var config = new Library.Config("https://localhost:7011");

    //    services.AddDbContext<JournalDbContext>(options =>
    //        options.UseSqlServer("Server=localhost;Database=JournalTest;Trusted_Connection=True;TrustServerCertificate=True;"));

    //    services.AddDbContext<IdentityContext>(options =>
    //        options.UseSqlServer("Server=localhost;Database=Identity;Trusted_Connection=True;TrustServerCertificate=True;"));


    //    services.RegisterAuthentication(config);
    //    var provider = services.BuildServiceProvider();
    //    var identityDbContext = provider.GetRequiredService<IdentityContext>();
    //    var journalDbContext = provider.GetRequiredService<JournalDbContext>();
    //    var authClient = provider.GetRequiredService<Library.Authentication.Interface>();

    //    var id= Guid.NewGuid();
    //    var payload = new Library.Authentication.Register.Payload
    //    {
    //        AccountName = "DUY",
    //        UserName = $"duy_{id}", 
    //        Email = $"duy_{id}@example.com",
    //        Password = "StrongPassword@123",
    //        ConfirmPassword = "StrongPassword@123",
    //        PhoneNumber = "0123456789",
    //    };
    //    await authClient.RegisterAsync(payload);
        
    //    var identityUser = await identityDbContext.Users.FirstOrDefaultAsync(u => u.Email == payload.Email);
    //    var user = await journalDbContext.Users.FirstOrDefaultAsync(u => u.Email == payload.Email);
    //    Assert.NotNull(identityUser); 
    //    Assert.NotNull(user); 
    //    Assert.Equal(payload.Email, identityUser?.Email);
    //    Assert.Equal(payload.UserName, $"duy_{id}");
    //    Assert.Equal(payload.PhoneNumber, user?.PhoneNumber);
    //    identityDbContext.Users.Remove(identityUser);
    //    journalDbContext.Users.Remove(user);
    //}
}
