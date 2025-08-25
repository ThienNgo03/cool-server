namespace Library.WorkoutLogSets.Implementations.Version1.Models.Refit.GET;

public class Data
{
    public Guid Id { get; set; }
    public Guid WorkoutLogId { get; set; }
    public int Value { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime LastUpdated { get; set; }
}
