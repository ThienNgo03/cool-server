namespace Journal.Notes.Get;
public class Parameters: Models.PaginationParameters.Model
{
    public Guid? journeyId { get; set; }
    public Guid? userId { get; set; }
    public string? content { get; set; }
    public string? mood { get; set; }
    public DateTime? date { get; set; }
}
