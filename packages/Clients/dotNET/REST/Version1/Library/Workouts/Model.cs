namespace Library.Workouts;

public class Model
{
    public Guid Id { get; set; }

    public Guid ExerciseId { get; set; }

    public Guid UserId { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime LastUpdated { get; set; }
}
