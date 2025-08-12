using System.ComponentModel.DataAnnotations;

namespace Library.TeamPools.Create;

public class Payload
{
    public int Position { get; set; }
    
    public Guid ParticipantId { get; set; }
    
    public Guid CompetitionId { get; set; }
}
