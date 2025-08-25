using Refit;

namespace Library.WorkoutLogSets.Implementations.Version1;

public interface IRefitInterface
{
    [Get("/api/workout-log-sets")]
    Task<ApiResponse<Models.Refit.GET.Response>> GET([Query] Models.Refit.GET.Parameters parameters);

    [Post("/api/workout-log-sets")]
    Task<ApiResponse<object>> POST([Body] Models.Refit.POST.Payload payload);

    [Put("/api/workout-log-sets")]
    Task<ApiResponse<object>> PUT([Body] Models.Refit.PUT.Payload payload);

    [Delete("/api/workout-log-sets")]
    Task<ApiResponse<object>> DELETE([Query] Models.Refit.DELETE.Parameters parameters);

}
