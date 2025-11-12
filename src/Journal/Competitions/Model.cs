namespace Journal.Competitions;

public class Model : Models.Base
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<Guid> ParticipantIds { get; set; } = [];
    public Guid ExerciseId { get; set; }
    public string Location { get; set; } = string.Empty;
    public DateTime DateTime { get; set; }
    public string Type { get; set; } = string.Empty;
    public Guid? RefereeId { get; set; }
}
