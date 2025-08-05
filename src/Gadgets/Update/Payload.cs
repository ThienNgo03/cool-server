namespace Journal.Gadgets.Update;

public class Payload
{
    public Guid Id { get; set; } // Unique identifier for the gadget
    public string Name { get; set; } // Name of the gadget
    public string Description { get; set; } // Description of the gadget
    public string Brand { get; set; } // Brand of the gadget
    public DateTime Date { get; set; }
}
