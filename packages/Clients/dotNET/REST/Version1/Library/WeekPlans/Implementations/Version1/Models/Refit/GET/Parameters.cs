namespace Library.WeekPlans.Implementations.Version1.Models.Refit.GET;

public class Parameters
{
    public Guid? Id { get; set; }

    public Guid? WorkoutId { get; set; }

    public string? DateOfWeek { get; set; }

    public TimeSpan? Time { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? LastUpdated { get; set; }

    public int? PageSize { get; set; }

    public int? PageIndex { get; set; }
}
