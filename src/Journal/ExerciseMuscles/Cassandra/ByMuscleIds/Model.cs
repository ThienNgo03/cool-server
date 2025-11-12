namespace Journal.ExerciseMuscles.CassandraTables.ByMuscleIds;

public class Model: Models.Base
{
    public Guid ExerciseId { get; set; }
    public Guid MuscleId { get; set; }
}
