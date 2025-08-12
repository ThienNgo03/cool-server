using System.ComponentModel.DataAnnotations;

namespace Library.SoloPools.Create;

public class Payload
{
    public Guid WinnerId { get; set; }
    
    public Guid LoserId { get; set; }
    
    public Guid CompetitionId { get; set; }
}
