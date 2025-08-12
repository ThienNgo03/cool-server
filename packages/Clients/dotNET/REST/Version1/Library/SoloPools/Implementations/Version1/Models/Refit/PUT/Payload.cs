namespace Library.SoloPools.Implementations.Version1.Models.Refit.PUT;

public class Payload
{
    public Guid Id { get; set; }

    public Guid WinnerId { get; set; }

    public Guid LoserId { get; set; }

    public Guid CompetitionId { get; set; }
}
