
namespace Library.SoloPools.Implementations.Version1.Models.Refit.GET;

public class Parameters
{
    public Guid? Id { get; set; }
    public Guid? WinnerId { get; set; }
    public Guid? LoserId { get; set; }
    public Guid? CompetitionId { get; set; }
    public DateTime? CreatedDate { get; set; }
    public int? PageIndex { get; set; }
    public int? PageSize { get; set; }
}
