namespace Library.WorkoutLogs.GET;

public class Parameters: Models.PaginationParameters.Model   
{
    public Guid? Id { get; set; }

    public Guid? WorkoutId { get; set; }

    public DateTime? WorkoutDate { get; set; }

}
