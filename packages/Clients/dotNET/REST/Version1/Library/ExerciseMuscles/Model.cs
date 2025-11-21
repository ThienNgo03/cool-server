namespace Library.ExerciseMuscles;

public class Model
{
    public Guid Id { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? LastUpdated { get; set; }
    public Guid ExerciseId { get; set; }
    public Guid MuscleId { get; set; }
}

