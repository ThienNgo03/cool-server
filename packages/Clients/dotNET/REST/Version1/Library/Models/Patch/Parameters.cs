namespace Library.Models.Patch;

public class Parameters
{
    public Guid Id { get; set; }
    public List<Operation> Operations { get; set; } = new();
}