namespace BFF.Chat.LoadMessage;

public class Parameters
{
    public Guid? Id { get; set; }
    public string? Content { get; set; } = string.Empty;
    public string? Receiver { get; set; } = string.Empty;
    public string? Sender { get; set; } = string.Empty;
}
