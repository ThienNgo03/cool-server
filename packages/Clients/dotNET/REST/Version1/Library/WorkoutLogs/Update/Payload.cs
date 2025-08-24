namespace Library.WorkoutLogs.Update;

public class Payload
{
    public Guid Id { get; set; }

    public Guid WorkoutId { get; set; }

    public DateTime WorkoutDate { get; set; }
}
