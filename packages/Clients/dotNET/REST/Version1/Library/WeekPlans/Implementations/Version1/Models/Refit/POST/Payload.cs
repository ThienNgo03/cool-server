namespace Library.WeekPlans.Implementations.Version1.Models.Refit.POST;

public class Payload
{
    public Guid WorkoutId { get; set; }

    public string DateOfWeek { get; set; }

    public DateTime Time { get; set; }
}
