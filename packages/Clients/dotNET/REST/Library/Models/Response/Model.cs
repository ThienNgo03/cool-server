namespace Library.Models.Response;

public class Model<T>
{
    public long? Duration { get; set; }
    public string? Title { get; set; }
    public string? Detail { get; set; }
    public bool IsSuccess { get; set; }
    public T? Data { get; set; }
}
