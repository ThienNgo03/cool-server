namespace Test.Databases.App.Tables.Gadget;

public class Table
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public DateTime Date { get; set; }
}
