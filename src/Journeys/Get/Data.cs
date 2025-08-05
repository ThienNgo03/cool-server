namespace Journal.Journeys.Get;


public class Data
{

    public Guid Id { get; set; } // cột ID

    public string Content { get; set; } // cột nội dung nhật ký

    public string? Location { get; set; }

    public DateTime Date { get; set; }
}
