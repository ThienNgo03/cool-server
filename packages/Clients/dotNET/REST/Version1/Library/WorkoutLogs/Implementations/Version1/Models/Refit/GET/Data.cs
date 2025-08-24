namespace Library.WorkoutLogs.Implementations.Version1.Models.Refit.GET;

public class Data
{
    public Guid Id { get; set; }

    public Guid WorkoutId { get; set; }

    public DateTime WorkoutDate { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime LastUpdated { get; set; }
}
