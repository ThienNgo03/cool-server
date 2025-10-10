using Grpc.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Test.Databases.Identity;
using Test.Databases.App;

namespace Test.Authentication;

public class Test : BaseTest
{
    #region [ CTors ]

    public Test() : base() { }   
    #endregion

    #region [ Endpoints ]

    [Fact]
    public async Task POST_SignIn_EmailLogin_ReturnsToken()
    {
        //Arrange
        var identityDbContext = serviceProvider.GetRequiredService<IdentityContext>();
        var journalDbContext = serviceProvider.GetRequiredService<JournalDbContext>();
        var authClient = serviceProvider.GetRequiredService<Library.Authentication.Interface>();
        //Data
        var id = Guid.NewGuid();
        var hasher = new PasswordHasher<IdentityUser>();
        var password = "StrongPassword@123";
        var newUser = new IdentityUser
        {
            Id = Guid.NewGuid().ToString(),
            UserName = $"duy_nguyen_{id}",
            Email = $"duy_nguyen_{id}@example.com",
            NormalizedEmail = $"duy_nguyen_{id}@example.com".ToUpper(),
            EmailConfirmed = true,
            PhoneNumber = "0918761646",
            PhoneNumberConfirmed = true,
            SecurityStamp = Guid.NewGuid().ToString(),
            ConcurrencyStamp = Guid.NewGuid().ToString()
        };
        newUser.PasswordHash = hasher.HashPassword(newUser, password);
        identityDbContext.Users.Add(newUser);
        await identityDbContext.SaveChangesAsync();

        journalDbContext.Users.Add(new Databases.App.Tables.User.Table
        {
            Id = Guid.NewGuid(),
            Name = newUser.UserName,
            Email = newUser.Email,
            PhoneNumber = newUser.PhoneNumber,
        });
        await journalDbContext.SaveChangesAsync();

        var payload = new Library.Authentication.Signin.Payload
        {
            Account = $"duy_nguyen_{id}@example.com",
            Password = "StrongPassword@123"
        };
        //Act
        var result = await authClient.SignInAsync(payload);
        //Assert
        Assert.NotNull(result);
        Assert.False(string.IsNullOrWhiteSpace(result.Token));
        //Clean up
        var expectedIdentityUser = await identityDbContext.Users.FirstOrDefaultAsync(u => u.Email == newUser.Email);
        var expectedUser = await journalDbContext.Users.FirstOrDefaultAsync(u => u.Email == newUser.Email);
        identityDbContext.Users.Remove(expectedIdentityUser);
        journalDbContext.Users.Remove(expectedUser);
    }

    [Fact]
    public async Task POST_SignIn_PhoneLogin_ReturnsToken()
    {
        //Arrange
        var identityDbContext = serviceProvider.GetRequiredService<IdentityContext>();
        var journalDbContext = serviceProvider.GetRequiredService<JournalDbContext>();
        var authClient = serviceProvider.GetRequiredService<Library.Authentication.Interface>();
        //Data
        var id = Guid.NewGuid();
        var hasher = new PasswordHasher<IdentityUser>();
        var password = "StrongPassword@123";
        var newUser = new IdentityUser
        {
            Id = Guid.NewGuid().ToString(),
            UserName = $"duy_nguyen_{id}",
            Email = $"duy_nguyen_{id}@example.com",
            NormalizedEmail = $"duy_nguyen_{id}@example.com".ToUpper(),
            EmailConfirmed = true,
            PhoneNumber = "0918761646",
            PhoneNumberConfirmed = true,
            SecurityStamp = Guid.NewGuid().ToString(),
            ConcurrencyStamp = Guid.NewGuid().ToString()
        };
        newUser.PasswordHash = hasher.HashPassword(newUser, password);
        identityDbContext.Users.Add(newUser);
        await identityDbContext.SaveChangesAsync();

        journalDbContext.Users.Add(new Databases.App.Tables.User.Table
        {
            Id = Guid.NewGuid(),
            Name = newUser.UserName,
            Email = newUser.Email,
            PhoneNumber = newUser.PhoneNumber,
        });
        await journalDbContext.SaveChangesAsync();

        var payload = new Library.Authentication.Signin.Payload
        {
            Account = $"0918761646",
            Password = "StrongPassword@123"
        };
        //Act
        var result = await authClient.SignInAsync(payload);
        //Assert
        Assert.NotNull(result);
        Assert.False(string.IsNullOrWhiteSpace(result.Token));
        //Clean up
        var expectedIdentityUser = await identityDbContext.Users.FirstOrDefaultAsync(u => u.Email == newUser.Email);
        var expectedUser = await journalDbContext.Users.FirstOrDefaultAsync(u => u.Email == newUser.Email);
        identityDbContext.Users.Remove(expectedIdentityUser);
        journalDbContext.Users.Remove(expectedUser);
    }

    [Fact]
    public async Task POST_SignIn_EmailLogin_InvalidPassword()
    {
        //Arrange
        var identityDbContext = serviceProvider.GetRequiredService<IdentityContext>();
        var journalDbContext = serviceProvider.GetRequiredService<JournalDbContext>();
        var authClient = serviceProvider.GetRequiredService<Library.Authentication.Interface>();
        //Data
        var id = Guid.NewGuid();
        var hasher = new PasswordHasher<IdentityUser>();
        var password = "StrongPassword@123";
        var newUser = new IdentityUser
        {
            Id = Guid.NewGuid().ToString(),
            UserName = $"duy_nguyen_{id}",
            Email = $"duy_nguyen_{id}@example.com",
            NormalizedEmail = $"duy_nguyen_{id}@example.com".ToUpper(),
            EmailConfirmed = true,
            PhoneNumber = "0918761646",
            PhoneNumberConfirmed = true,
            SecurityStamp = Guid.NewGuid().ToString(),
            ConcurrencyStamp = Guid.NewGuid().ToString()
        };
        newUser.PasswordHash = hasher.HashPassword(newUser, password);
        identityDbContext.Users.Add(newUser);
        await identityDbContext.SaveChangesAsync();

        journalDbContext.Users.Add(new Databases.App.Tables.User.Table
        {
            Id = Guid.NewGuid(),
            Name = newUser.UserName,
            Email = newUser.Email,
            PhoneNumber = newUser.PhoneNumber,
        });
        await journalDbContext.SaveChangesAsync();

        var payload = new Library.Authentication.Signin.Payload
        {
            Account = $"duy_nguyen_{id}@example.com",
            Password = "StrongPassword@1234"
        };
        //Act
        var ex = await Assert.ThrowsAsync<RpcException>(() => authClient.SignInAsync(payload));
        //Assert
        Assert.Equal(StatusCode.Unauthenticated, ex.StatusCode);
        Assert.Contains("Invalid email or password", ex.Status.Detail, StringComparison.OrdinalIgnoreCase);
        //Clean up
        var expectedIdentityUser = await identityDbContext.Users.FirstOrDefaultAsync(u => u.Email == newUser.Email);
        var expectedUser = await journalDbContext.Users.FirstOrDefaultAsync(u => u.Email == newUser.Email);
        identityDbContext.Users.Remove(expectedIdentityUser!);
        journalDbContext.Users.Remove(expectedUser!);
    }
    
    [Fact]
    public async Task POST_Register_ValidPayload_Succeeds()
    {
        //Arrange
        var identityDbContext = serviceProvider.GetRequiredService<IdentityContext>();
        var journalDbContext = serviceProvider.GetRequiredService<JournalDbContext>();
        var authClient = serviceProvider.GetRequiredService<Library.Authentication.Interface>();
        //Data
        var id = Guid.NewGuid();
        var payload = new Library.Authentication.Register.Payload
        {
            FirstName = "DUY",
            LastName="NGUYEN",
            UserName = $"duy_nguyen_{id}",
            Email = $"duy_nguyen_{id}@example.com",
            Password = "StrongPassword@123",
            ConfirmPassword = "StrongPassword@123",
            PhoneNumber = "0987667890",
        };
        await authClient.RegisterAsync(payload);
        //Act
        var expectedIdentityUser = await identityDbContext.Users.FirstOrDefaultAsync(u => u.Email == payload.Email);
        var expectedUser = await journalDbContext.Users.FirstOrDefaultAsync(u => u.Email == payload.Email);
        Assert.NotNull(expectedIdentityUser);
        Assert.NotNull(expectedUser);
        Assert.Equal(payload.Email, expectedIdentityUser?.Email);
        Assert.Equal(payload.UserName, $"duy_nguyen_{id}");
        Assert.Equal(payload.PhoneNumber, expectedUser?.PhoneNumber);
        //Clean up
        identityDbContext.Users.Remove(expectedIdentityUser!);
        journalDbContext.Users.Remove(expectedUser!);
    }

    #endregion
}
