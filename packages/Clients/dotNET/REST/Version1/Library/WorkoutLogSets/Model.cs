namespace Library.WorkoutLogSets;

public class Model
{
    public Guid Id { get; set; }
    public Guid WorkoutLogId { get; set; }
    public int Value { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? LastUpdated { get; set; }
}
