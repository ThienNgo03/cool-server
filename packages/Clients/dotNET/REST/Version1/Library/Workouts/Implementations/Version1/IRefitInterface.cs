using Refit;

namespace Library.Workouts.Implementations.Version1;

public interface IRefitInterface
{
    [Get("/api/workouts")]
    Task<ApiResponse<Models.PaginationResults.Model<Model>>> GET([Query] GET.Parameters parameters);

    [Post("/api/workouts")]
    Task<ApiResponse<object>> POST([Body] POST.Payload payload);

    [Put("/api/workouts")]
    Task<ApiResponse<object>> PUT([Body] PUT.Payload payload);

    [Delete("/api/workouts")]
    Task<ApiResponse<object>> DELETE([Query] DELETE.Parameters parameters);
}
