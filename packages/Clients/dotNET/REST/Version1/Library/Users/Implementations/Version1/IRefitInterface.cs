using Refit;

namespace Library.Users.Implementations.Version1;

public interface IRefitInterface
{
    [Get("/api/profiles")]
    Task<ApiResponse<Models.PaginationResults.Model<Model>>> GET([Query] GET.Parameters parameters);

    [Post("/api/profiles")]
    Task<ApiResponse<object>> POST([Body] POST.Payload payload);

    [Put("/api/profiles")]
    Task<ApiResponse<object>> PUT([Body] PUT.Payload payload);

    [Delete("/api/profiles")]
    Task<ApiResponse<object>> DELETE([Query] DELETE.Parameters parameters);
}
