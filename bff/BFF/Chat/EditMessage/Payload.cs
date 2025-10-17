namespace BFF.Chat.EditMessage;

public class Payload
{
    public int Month { get; set; }
    public Guid MessageId { get; set; }
    public string SentDate { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
