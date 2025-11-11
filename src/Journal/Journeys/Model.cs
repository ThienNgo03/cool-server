namespace Journal.Journeys;

public class Model : Models.Base
{
    public string Content { get; set; }

    public string? Location { get; set; }

    public DateTime Date { get; set; }
}
