namespace Journal.TeamPools.Get;

public class Parameters
{
    public Guid? Id { get; set; }
    public int? Position { get; set; }
    public Guid? ParticipantId { get; set; }
    public Guid? CompetitionId { get; set; }
    public DateTime? CreatedDate { get; set; }
    public int? PageIndex { get; set; }
    public int? PageSize { get; set; }
}
