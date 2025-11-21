namespace Core.Exercises.Configurations;

public interface Interface
{
    Task<Detail.Response> DetailAsync(Detail.Parameters parameters);
    Task SaveAsync(Save.Payload payload);
}
