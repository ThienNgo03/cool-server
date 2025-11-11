namespace Journal.Journeys.Update
{
    public class Payload
    {
        public Guid Id { get; set; } // cột ID

        public string Content { get; set; } // cột nội dung nhật ký

        public string? Location { get; set; } 

        public DateTime Date { get; set; } // cột ngày viết nhật ký này
    }
}
