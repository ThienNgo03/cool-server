using Refit;

namespace Library.WorkoutLogs.Implementations.Version1;

public interface IRefitInterface
{
    [Get("/api/workout-logs")]
    Task<ApiResponse<Models.PaginationResults.Model<Model>>> GET([Query] GET.Parameters parameters);

    [Post("/api/workout-logs")]
    Task<ApiResponse<object>> POST([Body] POST.Payload payload);

    [Put("/api/workout-logs")]
    Task<ApiResponse<object>> PUT([Body] PUT.Payload payload);

    [Patch("/api/workout-logs")]
    Task<ApiResponse<object>> PATCH([Query] PATCH.Parameters parameters, [Body] List<PATCH.Operation> operations);

    [Delete("/api/workout-logs")]
    Task<ApiResponse<object>> DELETE([Query] DELETE.Parameters parameters);
}
