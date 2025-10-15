using Refit;

namespace Core.ExerciseConfigurations.Implementations.Version1;

public interface IRefitInterface
{
    [Get("/api/exercise-configs/detail")]
    Task<ApiResponse<Models.Refit.Detail.Response>> Detail([Query] Models.Refit.Detail.Parameters parameters);

    [Post("/api/exercise-configs/save")]
    Task<ApiResponse<object>> Save([Body] Models.Refit.Save.Payload payload);
}
