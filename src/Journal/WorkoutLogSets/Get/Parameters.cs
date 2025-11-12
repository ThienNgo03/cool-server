namespace Journal.WorkoutLogSets.Get;

public class Parameters: Models.PaginationParameters.Model
{
    public Guid? WorkoutLogId { get; set; }

    public int? Value { get; set; }
}
