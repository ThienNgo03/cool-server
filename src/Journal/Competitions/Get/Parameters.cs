namespace Journal.Competitions.Get;

public class Parameters: Models.PaginationParameters.Model
{
    public string? Title { get; set; } = string.Empty;
    public string? Description { get; set; } = string.Empty;
    public string? ParticipantIds { get; set; } = string.Empty;
    public Guid? ExerciseId { get; set; }
    public string? Location { get; set; } = string.Empty;
    public DateTime? DateTime { get; set; }
    public string? Type { get; set; } = string.Empty;
    public Guid? RefereeId { get; set; }
}
