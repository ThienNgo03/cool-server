using Refit;

namespace Library.WorkoutLogSets.Implementations.Version1;

public interface IRefitInterface
{
    [Get("/api/workout-log-sets")]
    Task<ApiResponse<Models.PaginationResults.Model<Model>>> GET([Query] GET.Parameters parameters);

    [Post("/api/workout-log-sets")]
    Task<ApiResponse<object>> POST([Body] POST.Payload payload);

    [Put("/api/workout-log-sets")]
    Task<ApiResponse<object>> PUT([Body] PUT.Payload payload);

    [Patch("/api/workout-log-sets")]
    Task<ApiResponse<object>> PATCH([Query] PATCH.Parameters parameters, [Body] List<PATCH.Operation> operations);

    [Delete("/api/workout-log-sets")]
    Task<ApiResponse<object>> DELETE([Query] DELETE.Parameters parameters);
}

