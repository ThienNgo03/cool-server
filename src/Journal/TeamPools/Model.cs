namespace Journal.TeamPools;

public class Model : Models.Base
{
    public Guid ParticipantId { get; set; }
    public int Position { get; set; }
    public Guid CompetitionId { get; set; }
}
