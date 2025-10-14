using Cassandra.Mapping.Attributes;

namespace BFF.Databases.Messages;
[Table("messages")]
public class Table
{
    [PartitionKey]
    public Guid Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public string Receiver { get; set; } = string.Empty;
    public string Sender { get; set; } = string.Empty;
    public string ImageUploaded { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public int Day { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
}
