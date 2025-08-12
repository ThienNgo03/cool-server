using System.ComponentModel.DataAnnotations;

namespace Library.TeamPools.Implementations.Version1.Models.Refit.POST;

public class Payload
{
    public int Position { get; set; }
    
    public Guid ParticipantId { get; set; }
    
    public Guid CompetitionId { get; set; }
}
