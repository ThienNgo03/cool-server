namespace Library.SoloPools;

public class Model
{
    public Guid Id { get; set; }
    public Guid WinnerId { get; set; }
    public Guid LoserId { get; set; }
    public Guid CompetitionId { get; set; }
    public DateTime CreatedDate { get; set; }
}
