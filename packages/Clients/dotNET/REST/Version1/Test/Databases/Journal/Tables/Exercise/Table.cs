namespace Test.Databases.Journal.Tables.Exercise;

public class Table
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string MusclesWorked { get; set; } = string.Empty;

    public DateTime CreatedDate { get; set; }

    public DateTime LastUpdated { get; set; }

}
