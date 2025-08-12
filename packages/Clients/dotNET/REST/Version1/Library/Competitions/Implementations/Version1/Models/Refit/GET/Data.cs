namespace Library.Competitions.Implementations.Version1.Models.Refit.GET;

public class Data
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; } 
    public List<Guid> ParticipantIds { get; set; }
    public Guid ExerciseId { get; set; }
    public string Location { get; set; } 
    public DateTime DateTime { get; set; }
    public DateTime CreatedDate { get; set; }
    public string Type { get; set; }
}
