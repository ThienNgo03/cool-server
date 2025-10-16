using Refit;

namespace Core.Exercises.Implementations.Version1;

public interface IRefitInterface
{
    [Get("/api/exercises/all")]
    Task<ApiResponse<List<Models.Refit.All.Response>>> All([Query] Models.Refit.All.Parameters parameters);

    [Get("/api/exercises/categories")]
    Task<ApiResponse<List<string>>> Categories();
}
