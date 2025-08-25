namespace Library.WeekPlans;

public class Model
{
    public Guid Id { get; set; }

    public Guid WorkoutId { get; set; }

    public string DateOfWeek { get; set; }

    public TimeSpan Time { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? LastUpdated { get; set; }
}
