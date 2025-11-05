namespace Journal.Databases.OpenSearch.Indexes.Exercise;

public class Index
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Type { get; set; }
    public List<Muscle.Index> Muscles { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime LastUpdated { get; set; }
}
