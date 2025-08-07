
namespace Library.Exercises;

public interface Interface
{
    Task<Models.Response.Model<Models.PaginationResults.Model<Model>>> AllAsync(All.Parameters parameters);
}
