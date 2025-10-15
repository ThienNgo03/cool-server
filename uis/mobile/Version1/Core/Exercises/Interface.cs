namespace Core.Exercises;

public interface Interface
{
    Task<List<All.Response>> AllAsync(All.Parameters parameters);

    Task<List<string>> CategoriesAsync();
}
