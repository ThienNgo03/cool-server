using Library.Authentication.Signin;

namespace Library.Authentication.Implementations.Version2;

public class Implementation : Interface
{
    private readonly Authentication.Signin.Protos.LoginMethod.LoginMethodClient _loginClient;
    private readonly Authentication.Register.Protos.RegisterMethod.RegisterMethodClient _registerClient;
    public Implementation(
        Authentication.Signin.Protos.LoginMethod.LoginMethodClient loginClient,
        Authentication.Register.Protos.RegisterMethod.RegisterMethodClient registerClient)
    {
        _loginClient = loginClient;
        _registerClient = registerClient;
    }
    public async Task<Signin.Protos.Result?> GrpcSignInAsync(Signin.Protos.Payload payload)
    {
        var grpcPayload = new Authentication.Signin.Protos.Payload
        {
            Email = payload.Email,
            Password = payload.Password
        };

        var result = await _loginClient.LoginAsync(grpcPayload);

        return new Signin.Protos.Result
        {
            Token = result.Token
        };
    }

    public async Task GrpcRegisterAsync(Register.Protos.Payload payload)
    {
        var grpcPayload = new Authentication.Register.Protos.Payload
        {
            FirstName = payload.FirstName,
            LastName = payload.LastName,
            UserName = payload.UserName,
            Email = payload.Email,
            Password = payload.Password,
            ConfirmPassword = payload.ConfirmPassword,
            PhoneNumber = payload.PhoneNumber,
            ProfilePicturePath = string.Empty
        };

        await _registerClient.RegisterAsync(grpcPayload);
    }

    public Task<Response?> SignInAsync(Signin.Payload payload)
    {
        throw new NotImplementedException();
    }

    public Task RegisterAsync(Register.Payload payload)
    {
        throw new NotImplementedException();
    }
}
