namespace Library.WorkoutLogs.Implementations.Version1.Models.Refit.PUT;

public class Payload
{
    public Guid Id { get; set; }

    public Guid WorkoutId { get; set; }

    public DateTime WorkoutDate { get; set; }
}
