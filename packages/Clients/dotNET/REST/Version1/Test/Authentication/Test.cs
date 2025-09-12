using Grpc.Core;
using Library.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using Test.Constant;
using Test.Databases.Identity;
using Test.Databases.Journal;

namespace Test.Authentication;

public class Test : BaseTest
{
    #region [ CTors ]

    public Test()
    {

    }
    #endregion

    #region [ Endpoints ]
    [Fact]
    public async Task POST_SignIn_StandaloneTest_ReturnsToken()
    {
        //Arrange
        var identityDbContext = serviceProvider.GetRequiredService<IdentityContext>();
        var journalDbContext = serviceProvider.GetRequiredService<JournalDbContext>();
        var authClient = serviceProvider.GetRequiredService<Library.Authentication.Interface>();
        //Data
        var id = Guid.NewGuid();    
        var newUser = new Library.Authentication.Register.Protos.Payload
        {
            FirstName = "DUY",
            LastName = "NGUYEN",
            UserName = $"duy_nguyen_{id}",
            Email = $"duy_nguyen_{id}@example.com",
            Password = "StrongPassword@123",
            ConfirmPassword = "StrongPassword@123",
            PhoneNumber = "0123456789",
        };
        await authClient.GrpcRegisterAsync(newUser);
        
        var payload = new Library.Authentication.Signin.Protos.Payload
        {
            Email = $"duy_nguyen_{id}@example.com",
            Password = "StrongPassword@123"
        };
        //Act
        var result = await authClient.GrpcSignInAsync(payload);
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
    public async Task POST_SignIn_StandaloneTest_InvalidPassword()
    {
        //Arrange
        var identityDbContext = serviceProvider.GetRequiredService<IdentityContext>();
        var journalDbContext = serviceProvider.GetRequiredService<JournalDbContext>();
        var authClient = serviceProvider.GetRequiredService<Library.Authentication.Interface>();
        //Data
        var id = Guid.NewGuid();
        var newUser = new Library.Authentication.Register.Protos.Payload
        {
            FirstName = "DUY",
            LastName = "NGUYEN",
            UserName = $"duy_nguyen_{id}",
            Email = $"duy_nguyen_{id}@example.com",
            Password = "StrongPassword@123",
            ConfirmPassword = "StrongPassword@123",
            PhoneNumber = "0123456789",
        };
        await authClient.GrpcRegisterAsync(newUser);
        var config = new Library.Config("https://localhost:7011");
        var payload = new Library.Authentication.Signin.Protos.Payload
        {
            Email = $"duy_nguyen_{id}@example.com",
            Password = "StrongPassword@1234"
        };
        //Act
        var ex = await Assert.ThrowsAsync<RpcException>(() => authClient.GrpcSignInAsync(payload));
        //Assert
        Assert.Equal(StatusCode.Unauthenticated, ex.StatusCode);
        Assert.Contains("Invalid email or password", ex.Status.Detail, StringComparison.OrdinalIgnoreCase);
        //Clean up
        var expectedIdentityUser = await identityDbContext.Users.FirstOrDefaultAsync(u => u.Email == newUser.Email);
        var expectedUser = await journalDbContext.Users.FirstOrDefaultAsync(u => u.Email == newUser.Email);
        identityDbContext.Users.Remove(expectedIdentityUser);
        journalDbContext.Users.Remove(expectedUser);
    }
    #endregion
    [Fact]
    public async Task POST_Register_ValidPayload_Succeeds()
    {
        //Arrange
        var identityDbContext = serviceProvider.GetRequiredService<IdentityContext>();
        var journalDbContext = serviceProvider.GetRequiredService<JournalDbContext>();
        var authClient = serviceProvider.GetRequiredService<Library.Authentication.Interface>();
        //Data
        var id = Guid.NewGuid();
        var payload = new Library.Authentication.Register.Protos.Payload
        {
            FirstName = "DUY",
            LastName = "NGUYEN",
            UserName = $"duy_nguyen_{id}",
            Email = $"duy_nguyen_{id}@example.com",
            Password = "StrongPassword@123",
            ConfirmPassword = "StrongPassword@123",
            PhoneNumber = "0123456789",
        };
        await authClient.GrpcRegisterAsync(payload);
        //Act
        var expectedIdentityUser = await identityDbContext.Users.FirstOrDefaultAsync(u => u.Email == payload.Email);
        var expectedUser = await journalDbContext.Users.FirstOrDefaultAsync(u => u.Email == payload.Email);
        Assert.NotNull(expectedIdentityUser);
        Assert.NotNull(expectedUser);
        Assert.Equal(payload.Email, expectedIdentityUser?.Email);
        Assert.Equal(payload.UserName, $"duy_nguyen_{id}");
        Assert.Equal(payload.PhoneNumber, expectedUser?.PhoneNumber);
        //Clean up
        identityDbContext.Users.Remove(expectedIdentityUser);
        journalDbContext.Users.Remove(expectedUser);
    
    }
}
