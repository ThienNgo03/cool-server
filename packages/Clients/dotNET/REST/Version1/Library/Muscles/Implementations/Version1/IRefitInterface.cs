using Refit;

namespace Library.Muscles.Implementations.Version1;

public interface IRefitInterface
{
    [Get("/api/muscles")]
    Task<ApiResponse<Models.PaginationResults.Model<Model>>> GET([Query] GET.Parameters parameters);

    [Post("/api/muscles")]
    Task<ApiResponse<object>> POST([Body] POST.Payload payload);

    [Put("/api/muscles")]
    Task<ApiResponse<object>> PUT([Body] PUT.Payload payload);

    [Delete("/api/muscles")]
    Task<ApiResponse<object>> DELETE([Query] DELETE.Parameters parameters);
}
