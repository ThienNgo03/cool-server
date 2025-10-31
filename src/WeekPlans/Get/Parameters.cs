namespace Journal.WeekPlans.Get;

public class Parameters: Models.PaginationParameters.Model
{

    public Guid? WorkoutId { get; set; }

    public string? DateOfWeek { get; set; }

    public TimeSpan? Time { get; set; }
}
