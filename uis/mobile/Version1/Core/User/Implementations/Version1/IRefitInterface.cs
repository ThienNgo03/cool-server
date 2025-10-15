using Refit;

namespace Core.User.Implementations.Version1;

public interface IRefitInterface
{
    [Get("/api/users/all")]
    Task<ApiResponse<List<Models.Refit.All.Response>>> All([Query] Models.Refit.All.Parameters parameters);

}
