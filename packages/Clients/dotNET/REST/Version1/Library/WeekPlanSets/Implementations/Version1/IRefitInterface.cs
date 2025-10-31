using Refit;

namespace Library.WeekPlanSets.Implementations.Version1;

public interface IRefitInterface
{
    [Get("/api/week-plan-sets")]
    Task<ApiResponse<Models.PaginationResults.Model<Model>>> GET([Query] GET.Parameters parameters);

    [Post("/api/week-plan-sets")]
    Task<ApiResponse<object>> POST([Body] POST.Payload payload);

    [Put("/api/week-plan-sets")]
    Task<ApiResponse<object>> PUT([Body] PUT.Payload payload);

    [Patch("/api/week-plan-sets")]
    Task<ApiResponse<object>> PATCH([Query] PATCH.Parameters parameters, [Body] List<PATCH.Operation> operations);

    [Delete("/api/week-plan-sets")]
    Task<ApiResponse<object>> DELETE([Query] DELETE.Parameters parameters);
}
