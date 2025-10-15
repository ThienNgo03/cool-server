namespace Core.User.Implementations.Version1.Models.Refit.All;

public class Response
{
    public Guid UserId { get; set; }
    public string? Title { get; set; }
    public string? SubTitle { get; set; }
    public string? ImageUrl { get; set; }
    public TimeSpan Time { get; set; }
    public string? Status { get; set; }
}
