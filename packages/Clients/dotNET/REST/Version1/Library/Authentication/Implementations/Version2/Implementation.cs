using Library.Authentication.Signin;

namespace Library.Authentication.Implementations.Version2;

public class Implementation : Interface
{
    private readonly Journal.Beta.Authentication.Login.LoginMethod.LoginMethodClient _loginClient;
    private readonly Journal.Beta.Authentication.Register.RegisterMethod.RegisterMethodClient _registerClient;
    public Implementation(
        Journal.Beta.Authentication.Login.LoginMethod.LoginMethodClient loginClient,
        Journal.Beta.Authentication.Register.RegisterMethod.RegisterMethodClient registerClient)
    {
        _loginClient = loginClient;
        _registerClient = registerClient;
    }
    public async Task<Response?> SignInAsync(Signin.Payload payload)
    {
        var grpcPayload = new Journal.Beta.Authentication.Login.Payload
        {
            Email = payload.Account,
            Password = payload.Password
        };

        var result = await _loginClient.LoginAsync(grpcPayload);

        return new Response
        {
            Token = result.Token
        };
    }

    public async Task RegisterAsync(Register.Payload payload)
    {
        var grpcPayload = new Journal.Beta.Authentication.Register.Payload
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
