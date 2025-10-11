namespace BFF.Exercises.GetExercises;

public class Response 
{
    public Guid Id { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime LastUpdated { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Type { get; set; }
    public ICollection<Muscle>? Muscles { get; set; }
    
}

public class Muscle 
{
    public Guid Id { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime LastUpdated { get; set; }
    public string Name { get; set; }
}