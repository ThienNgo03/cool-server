using Refit;

namespace Core.Exercises.Configurations.Implementations.Version1;

public interface IRefitInterface
{
    [Get("/api/exercises/{id}/configs/detail")]
    Task<ApiResponse<Models.Refit.Detail.Response>> Detail([Query] Models.Refit.Detail.Parameters parameters, [AliasAs("id")] Guid exerciseId);

    [Post("/api/exercises/{id}/configs/save")]
    Task<ApiResponse<object>> Save([Body] Models.Refit.Save.Payload payload, [AliasAs("id")] Guid exerciseId);
}
