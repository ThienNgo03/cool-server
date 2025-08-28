using Refit;

namespace Library.WeekPlanSets.Implementations.Version1;

public interface IRefitInterface
{
    [Get("/api/week-plan-sets")]
    Task<ApiResponse<Models.Refit.GET.Response>> GET([Query] Models.Refit.GET.Parameters parameters);

    [Post("/api/week-plan-sets")]
    Task<ApiResponse<object>> POST([Body] Models.Refit.POST.Payload payload);

    [Patch("/api/reports")]
    Task<ApiResponse<object>> PATCH([Query] Models.Refit.PATCH.Parameters parameters, [Body] List<Models.Refit.PATCH.Operation> operations);

    [Put("/api/week-plan-sets")]
    Task<ApiResponse<object>> PUT([Body] Models.Refit.PUT.Payload payload);

    [Delete("/api/week-plan-sets")]
    Task<ApiResponse<object>> DELETE([Query] Models.Refit.DELETE.Parameters parameters);

}
