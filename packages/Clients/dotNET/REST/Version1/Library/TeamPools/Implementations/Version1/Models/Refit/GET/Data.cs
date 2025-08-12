namespace Library.TeamPools.Implementations.Version1.Models.Refit.GET;

public class Data
{
    public Guid Id { get; set; }
    public Guid ParticipantId { get; set; }
    public int Position { get; set; }
    public Guid CompetitionId { get; set; }
    public DateTime CreatedDate { get; set; }
}
