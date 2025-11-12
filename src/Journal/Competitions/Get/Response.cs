namespace Journal.Competitions.Get;

public class Response : Table
{
    public SoloPool? SoloPool { get; set; }
    public ICollection<TeamPool>? TeamPools { get; set; }
}

public class SoloPool
{
    public Guid Id { get; set; }
    public Guid WinnerId { get; set; }
    public Guid LoserId { get; set; }
    public Guid CompetitionId { get; set; }
    public DateTime CreatedDate { get; set; }
}

public class TeamPool
{
    public Guid Id { get; set; }
    public Guid ParticipantId { get; set; }
    public int Position { get; set; }
    public Guid CompetitionId { get; set; }
    public DateTime CreatedDate { get; set; }
}