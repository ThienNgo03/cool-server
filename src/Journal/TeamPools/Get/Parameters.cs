namespace Journal.TeamPools.Get;

public class Parameters: Models.PaginationParameters.Model
{
    public int? Position { get; set; }
    public Guid? ParticipantId { get; set; }
    public Guid? CompetitionId { get; set; }
}
