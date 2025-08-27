using System.ComponentModel.DataAnnotations;

namespace Journal.Competitions.Put;

public class Payload
{
    [Required]
    public Guid Id { get; set; }

    [Required]
    public string Title { get; set; } = string.Empty;
    
    
    public string Description { get; set; } = string.Empty;
    
    
    public string Location { get; set; } = string.Empty;

    public string? ParticipantIds { get; set; }


    public Guid ExerciseId { get; set; }
    
    
    public DateTime DateTime { get; set; }
    
    
    public string Type { get; set; }= string.Empty;

    public Guid? RefereeId { get; set; }
}
