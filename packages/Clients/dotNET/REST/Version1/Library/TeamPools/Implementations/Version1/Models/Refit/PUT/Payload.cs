using System.ComponentModel.DataAnnotations;

namespace Library.TeamPools.Implementations.Version1.Models.Refit.PUT;

public class Payload
{
    public Guid Id { get; set; }
    public Guid ParticipantId { get; set; }
    public int Position { get; set; }   
    public Guid CompetitionId { get; set; }
}
