using System.ComponentModel.DataAnnotations;

namespace Library.SoloPools.Update;

public class Payload
{
    public Guid Id { get; set; }
    
    public Guid WinnerId { get; set; }
    
    public Guid LoserId { get; set; }
    
    public Guid CompetitionId { get; set; }

}
