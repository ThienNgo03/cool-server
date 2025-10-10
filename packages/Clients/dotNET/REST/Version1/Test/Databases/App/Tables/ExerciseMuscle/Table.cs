namespace Test.Databases.App.Tables.ExerciseMuscle;

public class Table
{
    public Guid Id { get; set; }
    
    public Guid ExerciseId { get; set; }
    
    public Guid MuscleId { get; set; }
    
    public DateTime CreatedDate { get; set; }
    
    public DateTime LastUpdated { get; set; }
}
