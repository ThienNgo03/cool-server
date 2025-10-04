namespace BFF.Messages.PUT;

public class Payload
{
    public Guid Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public string Receiver { get; set; } = string.Empty;
    public string Sender { get; set; } = string.Empty;
}
