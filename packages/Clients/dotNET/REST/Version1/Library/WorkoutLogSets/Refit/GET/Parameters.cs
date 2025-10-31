namespace Library.WorkoutLogSets.GET;

public class Parameters
{
    public Guid? Id { get; set; }

    public Guid? UserId { get; set; }

    public Guid? WorkoutLogId { get; set; }

    public int? Value { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? LastUpdated { get; set; }

    public int? PageSize { get; set; }

    public int? PageIndex { get; set; }
}