using Refit;

namespace Library.WorkoutLogs.Implementations.Version1;

public interface IRefitInterface
{
    [Get("/api/workout-logs")]
    Task<ApiResponse<Models.Refit.GET.Response>> GET([Query] Models.Refit.GET.Parameters parameters);

    [Post("/api/workout-logs")]
    Task<ApiResponse<object>> POST([Body] Models.Refit.POST.Payload payload);

    [Put("/api/workout-logs")]
    Task<ApiResponse<object>> PUT([Body] Models.Refit.PUT.Payload payload);

    [Delete("/api/workout-logs")]
    Task<ApiResponse<object>> DELETE([Query] Models.Refit.DELETE.Parameters parameters);
}
