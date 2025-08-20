namespace Journal.Models.BaseTable;

public class Model
{
    public Guid Id { get; set; }
    public DateTime CreateAt { get; set; }
    public Guid InsertedBy { get; set; }
    public DateTime? LastUpdated { get; set; }
    public Guid UpdatedBy { get; set; }
}
