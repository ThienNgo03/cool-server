using System.ComponentModel.DataAnnotations;

namespace Journal.TeamPools.Put;

public class Payload
{
    [Required]
    public Guid Id { get; set; }
    public Guid ParticipantId { get; set; }
    public int Position { get; set; }
    [Required]
    public Guid CompetitionId { get; set; }
}
