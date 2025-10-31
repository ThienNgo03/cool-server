namespace Library.WorkoutLogSets.PATCH;

public class Operation
{
    public string op { get; set; } = string.Empty; 
    public string path { get; set; } = string.Empty; 
    public object? value { get; set; }
}
