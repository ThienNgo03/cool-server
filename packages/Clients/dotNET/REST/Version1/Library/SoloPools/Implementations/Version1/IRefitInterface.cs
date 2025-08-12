using Refit;

namespace Library.SoloPools.Implementations.Version1;

public interface IRefitInterface
{
    [Get("/api/solo-pools")]
    Task<ApiResponse<Models.Refit.GET.Response>> GET([Query] Models.Refit.GET.Parameters parameters);

    [Post("/api/solo-pools")]
    Task<ApiResponse<object>> POST([Body] Models.Refit.POST.Payload payload);

    [Put("/api/solo-pools")]
    Task<ApiResponse<object>> PUT([Body] Models.Refit.PUT.Payload payload);

    [Delete("/api/solo-pools")]
    Task<ApiResponse<object>> DELETE([Query] Models.Refit.DELETE.Parameters parameters);
}
