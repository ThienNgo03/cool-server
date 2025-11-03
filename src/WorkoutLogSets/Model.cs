namespace Journal.WorkoutLogSets;

public class Model: Models.Base
{
    public Guid WorkoutLogId { get; set; }
    public int Value { get; set; }
}