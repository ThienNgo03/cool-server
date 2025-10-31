namespace Library.WorkoutLogSets.PATCH;

public class PatchDocument
{
    public List<Operation> Operations { get; set; } = new();
}