using System.ComponentModel.DataAnnotations;

namespace Journal.Competitions.Post;

public class Payload
{
    [Required]
    public string Title { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;
    
    public string Location { get; set; } = string.Empty;

    public string? ParticipantIds { get; set; }

    public Guid ExerciseId { get; set; }
    
    public DateTime DateTime { get; set; }

    public Guid? RefereeId { get; set; }
    
    public string Type { get; set; } = string.Empty;
}
