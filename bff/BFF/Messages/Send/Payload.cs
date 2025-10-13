namespace BFF.Messages.Send;

public class Payload
{
    public string Content { get; set; } = string.Empty;
    public string Receiver { get; set; } = string.Empty;
    public string Sender { get; set; } = string.Empty;
}
