namespace Library.WeekPlans.POST;

public class Payload
{
    public Guid WorkoutId { get; set; }

    public string DateOfWeek { get; set; }

    public TimeSpan Time { get; set; }
}
