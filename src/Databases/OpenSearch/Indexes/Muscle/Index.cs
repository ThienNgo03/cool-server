namespace Journal.Databases.OpenSearch.Indexes.Muscle;

public class Index
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? LastUpdated { get; set; }
}
