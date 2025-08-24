namespace Library.WeekPlans.Implementations.Version1.Models.Refit.PUT;

public class Payload
{
    public Guid Id { get; set; }

    public Guid WorkoutId { get; set; }

    public string DateOfWeek { get; set; }

    public DateTime Time { get; set; }

}
