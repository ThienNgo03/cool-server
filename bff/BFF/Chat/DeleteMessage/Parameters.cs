namespace BFF.Chat.DeleteMessage;

public class Parameters
{
    public int Month { get; set; }
    public Guid MessageId { get; set; }
    public string SentDate { get; set; } = string.Empty;
}
