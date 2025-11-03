using Refit;

namespace Library.Users.Implementations.Version1;

public interface IRefitInterface
{
    [Get("/api/users")]
    Task<ApiResponse<Models.PaginationResults.Model<Model>>> GET([Query] GET.Parameters parameters);

    [Post("/api/users")]
    Task<ApiResponse<object>> POST([Body] POST.Payload payload);

    [Put("/api/users")]
    Task<ApiResponse<object>> PUT([Body] PUT.Payload payload);

    [Delete("/api/users")]
    Task<ApiResponse<object>> DELETE([Query] DELETE.Parameters parameters);
}
