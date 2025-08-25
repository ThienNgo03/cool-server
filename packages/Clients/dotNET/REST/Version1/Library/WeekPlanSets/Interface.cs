namespace Library.WeekPlanSets;

public interface Interface
{
    Task<Models.Response.Model<Models.PaginationResults.Model<Model>>> AllAsync(All.Parameters parameters);

    Task CreateAsync(Create.Payload payload);

    Task UpdateAsync(Update.Payload payload);

    Task DeleteAsync(Delete.Parameters parameters);
}
