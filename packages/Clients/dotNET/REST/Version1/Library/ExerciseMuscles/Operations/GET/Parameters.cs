namespace Library.ExerciseMuscles.GET;

public class Parameters : Models.PaginationParameters.Model
{
    public string? ExerciseId { get; set; }

    public Guid? PartitionKey { get; set; }
}
