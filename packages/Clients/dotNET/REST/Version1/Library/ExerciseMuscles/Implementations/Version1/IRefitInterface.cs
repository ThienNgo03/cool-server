using Refit;

namespace Library.ExerciseMuscles.Implementations.Version1;

public interface IRefitInterface
{
    [Get("/api/exercise-muscles")]
    Task<ApiResponse<Models.PaginationResults.Model<Model>>> GET([Query] GET.Parameters parameters);

    [Post("/api/exercise-muscles")]
    Task<ApiResponse<object>> POST([Body] POST.Payload payload);

    [Put("/api/exercise-muscles")]
    Task<ApiResponse<object>> PUT([Body] PUT.Payload payload);

    [Delete("/api/exercise-muscles")]
    Task<ApiResponse<object>> DELETE([Query] DELETE.Parameters parameters);
}
