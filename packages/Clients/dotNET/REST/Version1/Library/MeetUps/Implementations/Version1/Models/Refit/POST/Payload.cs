namespace Library.MeetUps.Implementations.Version1.Models.Refit.POST;

public class Payload
{
    public string ParticipantIds { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public DateTime DateTime { get; set; }

    public string Location { get; set; } = string.Empty;

    public string CoverImage { get; set; } = string.Empty;

}
