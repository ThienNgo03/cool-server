namespace Library.WeekPlanSets;

public interface Interface
{
    Task<Models.Response.Model<Models.PaginationResults.Model<Model>>> AllAsync(All.Parameters parameters);

    Task CreateAsync(Create.Payload payload);

    Task UpdateAsync(Update.Payload payload);

    Task PatchAsync(Models.Patch.Parameters parameters);

    Task DeleteAsync(Delete.Parameters parameters);
}
