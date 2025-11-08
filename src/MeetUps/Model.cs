namespace Journal.MeetUps;

public class Model : Models.Base
{
    public string ParticipantIds { get; set; }
    public string Title { get; set; }
    public DateTime DateTime { get; set; }
    public string Location { get; set; }
    public string CoverImage { get; set; }
}
