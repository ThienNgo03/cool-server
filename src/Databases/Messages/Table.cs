using Cassandra.Mapping.Attributes;

namespace Journal.Databases.Messages;
[Table("message_by_month")]
public class Table
{
    public Guid Id { get; set; }
    public string Avatar { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string SentDate { get; set; } = string.Empty;
    public int Day { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
}
