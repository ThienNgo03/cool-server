namespace Library.WeekPlanSets.Implementations.Version1.Models.Refit.GET;

public class Data
{
    public Guid Id { get; set; }

    public Guid WeekPlanId { get; set; }

    public int Value { get; set; }

    public Guid InsertedBy { get; set; }

    public Guid UpdatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? LastUpdated { get; set; }
}
