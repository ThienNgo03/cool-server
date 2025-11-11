namespace Journal.WorkoutLogs;

public class Model: Models.Base
{
    public Guid WorkoutId { get; set; }

    public DateTime WorkoutDate { get; set; }
}
