using Refit;

namespace Core.Authentication.Implementations.Version1;

public interface IRefitInterface
{
    [Post("/api/authentication/register")]
    Task<ApiResponse<object>> Register([Body] Models.Refit.Register.Payload payload);

    [Post("/api/authentication/login")]
    Task<ApiResponse<Models.Refit.SignIn.Response>> SignIn([Body] Models.Refit.SignIn.Payload payload);

}
