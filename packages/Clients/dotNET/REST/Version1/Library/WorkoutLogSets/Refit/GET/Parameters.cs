namespace Library.WorkoutLogSets.GET;

public class Parameters:Models.PaginationParameters.Model
{
    public Guid? UserId { get; set; }

    public Guid? WorkoutLogId { get; set; }

    public int? Value { get; set; }
}