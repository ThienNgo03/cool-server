namespace Library.MeetUps.Implementations.Version1.Models.Refit.GET;

public class Parameters
{
    public Guid Id { get; set; }

    public string ParticipantIds { get; set; }

    public string Title { get; set; }

    public DateTime DateTime { get; set; }

    public string Location { get; set; }

    public string CoverImage { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? LastUpdated { get; set; }

    public int? PageSize { get; set; }

    public int? PageIndex { get; set; }

}
