namespace Core.Authentication;

public interface Interface
{
    Task<Signin.Response?> SignInAsync(Signin.Payload payload);
    Task RegisterAsync(Register.Payload payload);
}
