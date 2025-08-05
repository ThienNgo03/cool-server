
using Refit;

namespace Library.Exercises.Implementations.Version1;

public interface IRefitInterface
{
    [Get("/Exercises")]
    Task<ApiResponse<Models.Refit.GET.Response>> GET([Query] Models.Refit.GET.Parameters parameters);
}
