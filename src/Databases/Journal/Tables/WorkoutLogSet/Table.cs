namespace Journal.Databases.Journal.Tables.WorkoutLogSet;

public class Table
{
    public Guid Id { get; set; }
    public Guid WorkoutLogId { get; set; }
    public int Rep { get; set; }
    public TimeSpan HoldingTime { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime LastUpdated { get; set; }
}
