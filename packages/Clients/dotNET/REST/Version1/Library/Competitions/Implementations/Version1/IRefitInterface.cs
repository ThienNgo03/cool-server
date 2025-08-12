using Refit;

namespace Library.Competitions.Implementations.Version1;

public interface IRefitInterface
{
    [Get("/api/competitions")]
    Task<ApiResponse<Models.Refit.GET.Response>> GET([Query] Models.Refit.GET.Parameters parameters);

    [Post("/api/competitions")]
    Task<ApiResponse<object>> POST([Body] Models.Refit.POST.Payload payload);

    [Put("/api/competitions")]
    Task<ApiResponse<object>> PUT([Body] Models.Refit.PUT.Payload payload);

    [Delete("/api/competitions")]
    Task<ApiResponse<object>> DELETE([Query] Models.Refit.DELETE.Parameters parameters);
}
