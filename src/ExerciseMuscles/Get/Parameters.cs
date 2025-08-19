namespace Journal.ExerciseMuscles.Get;

public class Parameters
{
    public Guid? Id { get; set; }

    public Guid? ExerciseId { get; set; }

    public Guid? MuscleId { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? LastUpdated { get; set; }

    public int? PageSize { get; set; }

    public int? PageIndex { get; set; }

}
