
namespace Library.Authentication.Implementations.Version2;

public class Implementation : Interface
{
    private readonly Signin.Protos.LoginMethod.LoginMethodClient _loginClient;
    private readonly Register.Protos.RegisterMethod.RegisterMethodClient _registerClient;
    public Implementation(
        Signin.Protos.LoginMethod.LoginMethodClient loginClient,
        Register.Protos.RegisterMethod.RegisterMethodClient registerClient)
    {
        _loginClient = loginClient;
        _registerClient = registerClient;
    }
    public async Task<Signin.Response?> SignInAsync(Signin.Payload payload)
    {
        var grpcPayload = new Signin.Protos.Payload
        {
            Email = payload.Account,
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
        var grpcPayload = new Register.Protos.Payload
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
