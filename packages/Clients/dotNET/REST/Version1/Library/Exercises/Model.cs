namespace Library.Exercises;

public class Model
{

    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;

    public DateTime CreatedDate { get; set; }

    public DateTime LastUpdated { get; set; }
}
