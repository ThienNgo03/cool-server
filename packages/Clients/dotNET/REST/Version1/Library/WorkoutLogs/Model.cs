namespace Library.WorkoutLogs;

public class Model
{
    public Guid Id { get; set; }

    public Guid WorkoutId { get; set; }

    public int Rep { get; set; }

    public TimeSpan HoldingTime { get; set; }

    public int Set { get; set; }

    public DateTime WorkoutDate { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime LastUpdated { get; set; }
}
