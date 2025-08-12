namespace Library.Competitions.Implementations.Version1.Models.Refit.POST;

public class Payload
{
    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Location { get; set; } = string.Empty;

    public Guid ExerciseId { get; set; }

    public DateTime DateTime { get; set; }

    public string Type { get; set; } = string.Empty;
}
