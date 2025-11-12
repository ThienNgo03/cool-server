namespace Journal.SoloPools;

public class Model : Models.Base
{
    public Guid WinnerId { get; set; }
    public Guid LoserId { get; set; }
    public Guid CompetitionId { get; set; }
}
