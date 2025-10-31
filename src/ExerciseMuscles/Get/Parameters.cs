namespace Journal.ExerciseMuscles.Get;

public class Parameters: Models.PaginationParameters.Model
{

    public string? ExerciseId { get; set; }

    public Guid? PartitionKey { get; set; }

}
