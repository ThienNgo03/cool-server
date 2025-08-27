namespace Journal.Competitions.Get;

public class Parameters
{
    public Guid? Id { get; set; }
    public string? Title { get; set; } = string.Empty;
    public string? Description { get; set; } = string.Empty;
    public string? ParticipantIds { get; set; } = string.Empty;
    public Guid? ExerciseId { get; set; }
    public string? Location { get; set; } = string.Empty;
    public DateTime? DateTime { get; set; }
    public DateTime? CreatedDate { get; set; }
    public string? Type { get; set; } = string.Empty;
    public int? PageSize { get; set; }
    public int? PageIndex { get; set; }
    public Guid? RefereeId { get; set; }
    public bool IsIncludeTeamPools { get; set; } = false;
    public bool IsIncludeSoloPool { get; set; } = false;
}
