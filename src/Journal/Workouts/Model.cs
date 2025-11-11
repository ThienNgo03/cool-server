namespace Journal.Workouts;

public class Model: Models.Base
{
    public Guid ExerciseId { get; set; }
    public Guid UserId { get; set; }
}
