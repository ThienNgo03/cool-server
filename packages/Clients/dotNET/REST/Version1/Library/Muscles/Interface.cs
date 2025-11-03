namespace Library.Muscles;

public interface Interface
{
    Task<Models.PaginationResults.Model<Model>> AllAsync(GET.Parameters? parameters = null);
    Task CreateAsync(POST.Payload payload);
    Task UpdateAsync(PUT.Payload payload);
    Task DeleteAsync(DELETE.Parameters parameters);
}
