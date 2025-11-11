namespace Journal.Notes;

public class Model : Models.Base
{
    public Guid JourneyId { get; set; }
    public Guid UserId { get; set; }
    public string Content { get; set; }
    public string Mood { get; set; }
    public DateTime Date { get; set; }
}
