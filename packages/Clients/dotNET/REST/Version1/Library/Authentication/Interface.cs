namespace Library.Authentication;

public interface Interface
{
    Task<Signin.Response?> SignInAsync(Signin.Payload payload);
    Task RegisterAsync(Register.Payload payload);
    Task<Signin.Protos.Result?> GrpcSignInAsync(Signin.Protos.Payload payload);
    Task GrpcRegisterAsync(Register.Protos.Payload payload);
}
