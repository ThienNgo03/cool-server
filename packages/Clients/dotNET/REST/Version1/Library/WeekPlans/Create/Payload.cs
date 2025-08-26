namespace Library.WeekPlans.Create;

public class Payload
{
    public Guid WorkoutId { get; set; }

    public string DateOfWeek { get; set; } = string.Empty;

    public TimeSpan Time { get; set; }
}
