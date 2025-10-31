namespace Journal.ExerciseMuscles.Get;

public class Parameters
{
    public string? Ids { get; set; }

    public string? ExerciseId { get; set; }

    public Guid? PartitionKey { get; set; }

    public int? PageSize { get; set; }

    public int? PageIndex { get; set; }

}
