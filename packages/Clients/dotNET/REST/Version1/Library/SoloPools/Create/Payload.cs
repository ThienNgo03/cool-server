using System.ComponentModel.DataAnnotations;

namespace Library.SoloPools.Create;

public class Payload
{
    [Required]
    public Guid WinnerId { get; set; }

    [Required]
    public Guid LoserId { get; set; }

    [Required]
    public Guid CompetitionId { get; set; }
}
