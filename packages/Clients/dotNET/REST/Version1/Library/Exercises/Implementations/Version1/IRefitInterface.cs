using Refit;

namespace Library.Exercises.Implementations.Version1;

public interface IRefitInterface
{
    [Get("/api/exercises")]
    Task<ApiResponse<Models.PaginationResults.Model<Model>>> GET([Query] GET.Parameters parameters);

    [Post("/api/exercises")]
    Task<ApiResponse<object>> POST([Body] POST.Payload payload);

    [Put("/api/exercises")]
    Task<ApiResponse<object>> PUT([Body] PUT.Payload payload);

    [Delete("/api/exercises")]
    Task<ApiResponse<object>> DELETE([Query] DELETE.Parameters parameters);
}
