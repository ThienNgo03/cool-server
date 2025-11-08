namespace Journal.Gadgets;

public class Model : Models.Base
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public DateTime Date { get; set; }
}
