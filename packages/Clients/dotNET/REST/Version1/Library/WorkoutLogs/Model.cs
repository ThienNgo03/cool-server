namespace Library.WorkoutLogs;

public class Model
{
    public Guid Id { get; set; }

    public Guid WorkoutId { get; set; }

    public DateTime WorkoutDate { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime LastUpdated { get; set; }
}
