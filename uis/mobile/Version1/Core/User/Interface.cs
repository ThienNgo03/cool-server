namespace Core.User;

public interface Interface
{
    Task<List<All.Response>> AllAsync(All.Parameters parameters);
}
