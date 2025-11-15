namespace Journal.SoloPools.Get;

public class Parameters: Models.PaginationParameters.Model
{
    public Guid? WinnerId { get; set; }
    public Guid? LoserId { get; set; }
    public Guid? CompetitionId { get; set; }
}
