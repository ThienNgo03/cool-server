using System.ComponentModel.DataAnnotations;

namespace Journal.Gadgets.Post;

public class Payload
{
    [Required]
    public string Name { get; set; } =string.Empty; // Name of the gadget
    public string Description { get; set; } =string.Empty; // Description of the gadget
    [Required]
    public string Brand { get; set; } =string.Empty; // Brand of the gadget

    public DateTime Date { get; set; }
}
