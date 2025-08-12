using Refit;

namespace Library.TeamPools.Implementations.Version1;

public interface IRefitInterface
{
    [Get("/api/teamPools")]
    Task<ApiResponse<Models.Refit.GET.Response>> GET([Query] Models.Refit.GET.Parameters parameters);

    [Post("/api/teamPools")]
    Task<ApiResponse<object>> POST([Body] Models.Refit.POST.Payload payload);

    [Put("/api/teamPools")]
    Task<ApiResponse<object>> PUT([Body] Models.Refit.PUT.Payload payload);

    [Delete("/api/teamPools")]
    Task<ApiResponse<object>> DELETE([Query] Models.Refit.DELETE.Parameters parameters);
}
