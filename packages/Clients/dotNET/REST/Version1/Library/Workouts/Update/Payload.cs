namespace Library.Workouts.Update;

public class Payload
{
    public Guid Id { get; set; }

    public Guid ExerciseId { get; set; }

    public Guid UserId { get; set; }
}
