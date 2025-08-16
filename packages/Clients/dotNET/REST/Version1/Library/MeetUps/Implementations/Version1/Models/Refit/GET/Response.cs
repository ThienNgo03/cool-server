namespace Library.MeetUps.Implementations.Version1.Models.Refit.GET;

public class Response
{
    public int Total { get; set; } = 0;
    public int? Index { get; set; }
    public int? Size { get; set; }
    public ICollection<Data>? Items { get; set; }

}
