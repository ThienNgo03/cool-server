namespace Journal.ExerciseMuscles.Get;

public class Parameters: Models.PaginationParameters.Model
{

    public Guid? ExerciseId { get; set; }

    public Guid? MuscleId { get; set; }

}
