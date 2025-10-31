using Refit;

namespace Library.WeekPlans.Implementations.Version1;

public interface IRefitInterface
{
    [Get("/api/week-plans")]
    Task<ApiResponse<Models.PaginationResults.Model<Model>>> GET([Query] GET.Parameters parameters);

    [Post("/api/week-plans")]
    Task<ApiResponse<object>> POST([Body] POST.Payload payload);

    [Put("/api/week-plans")]
    Task<ApiResponse<object>> PUT([Body] PUT.Payload payload);

    [Patch("/api/week-plans")]
    Task<ApiResponse<object>> PATCH([Query] PATCH.Parameters parameters, [Body] List<PATCH.Operation> operations);

    [Delete("/api/week-plans")]
    Task<ApiResponse<object>> DELETE([Query] DELETE.Parameters parameters);
}
