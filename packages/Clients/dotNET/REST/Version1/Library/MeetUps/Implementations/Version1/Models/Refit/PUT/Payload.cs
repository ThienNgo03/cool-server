namespace Library.MeetUps.Implementations.Version1.Models.Refit.PUT;

public class Payload
{
    public Guid Id { get; set; }

    public string ParticipantIds { get; set; }

    public string Title { get; set; }

    public DateTime DateTime { get; set; }

    public string Location { get; set; }

    public string CoverImage { get; set; }

}
