namespace Journal.Models.BaseTable;

public class Model
{
    public Guid Id { get; set; }
    public DateTime CreatedDate { get; set; }
    public Guid InsertedBy { get; set; }
    public DateTime? LastUpdated { get; set; }
    public Guid UpdatedBy { get; set; }
}
