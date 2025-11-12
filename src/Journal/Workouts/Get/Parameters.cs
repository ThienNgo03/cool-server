namespace Journal.Workouts.Get;

public class Parameters: Models.PaginationParameters.Model
{
    public Guid? ExerciseId { get; set; }

    public Guid? UserId { get; set; }
}
