namespace Journal.Models;

public class Base
{
    public Guid Id { get; set; }
    public DateTime CreatedDate { get; set; }
    public Guid CreatedById { get; set; }
    public DateTime? LastUpdated { get; set; }
    public Guid? UpdatedById { get; set; }
}
