namespace Journal.WeekPlans;

public class Model: Models.Base
{
    public Guid WorkoutId { get; set; }

    public string DateOfWeek { get; set; }

    public TimeSpan Time { get; set; }
}
