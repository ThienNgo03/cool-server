
using Refit;

namespace Library.Exercises.Implementations.Version1;

public interface IRefitInterface
{
    [Get("/api/exercises")]
    Task<ApiResponse<Models.Refit.GET.Response>> GET([Query] Models.Refit.GET.Parameters parameters);
}
