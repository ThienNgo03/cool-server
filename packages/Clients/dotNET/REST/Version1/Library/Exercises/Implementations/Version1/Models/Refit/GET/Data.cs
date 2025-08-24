namespace Library.Exercises.Implementations.Version1.Models.Refit.GET;

public class Data
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;

    public DateTime CreatedDate { get; set; }

    public DateTime LastUpdated { get; set; }
}
