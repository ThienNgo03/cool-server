namespace Journal.ExerciseMuscles.Tables.CassandraTables.ByExerciseIds;

public class Model: Models.Base
{
    public Guid ExerciseId { get; set; }
    public Guid MuscleId { get; set; }
}
