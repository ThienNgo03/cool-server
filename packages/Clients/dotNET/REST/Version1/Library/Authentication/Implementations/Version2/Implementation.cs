
namespace Library.Authentication.Implementations.Version2;

public class Implementation : Interface
{
    private readonly Protos.LoginMethod.LoginMethodClient _loginClient;
    private readonly Protos.RegisterMethod.RegisterMethodClient _registerClient;
    public Implementation(
        Protos.LoginMethod.LoginMethodClient loginClient,
        Protos.RegisterMethod.RegisterMethodClient registerClient)
    {
        _loginClient = loginClient;
        _registerClient = registerClient;
    }
    public async Task<Signin.Response?> SignInAsync(Signin.Payload payload)
    {
        var grpcPayload = new Protos.LoginPayload
        {
            Account = payload.Account,
            Password = payload.Password
        };

        var result = await _loginClient.LoginAsync(grpcPayload);

        return new Signin.Response
        {
            Token = result.Token
        };
    }

    public async Task RegisterAsync(Register.Payload payload)
    {
        var grpcPayload = new Version2.Protos.RegisterPayload
        {
            FirstName = payload.AccountName,
            LastName = payload.AccountName,
            UserName = payload.UserName,
            Email = payload.Email,
            Password = payload.Password,
            ConfirmPassword = payload.ConfirmPassword,
            PhoneNumber = payload.PhoneNumber,
            ProfilePicturePath = string.Empty
        };

        await _registerClient.RegisterAsync(grpcPayload);
    }

}
