namespace Journal.Gadgets.Get;

public class Parameters
{
    public int? PageIndex { get; set; }
    public int? PageSize { get; set; }
    public Guid? Id { get; set; } 
    public string? Name { get; set; } 
    public string? Brand { get; set; } 
    public string? Description { get; set; } 
    public DateTime? Date { get; set; }
}