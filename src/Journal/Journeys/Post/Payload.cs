using System.ComponentModel.DataAnnotations;

namespace Journal.Journeys.Post
{
    public class Payload // class đại diện cho những thứ mà user sẽ đưa lên server
    {
        public string Content { get; set; }// nội dung nhật ký, Required ở đây?

        public string? Location { get; set; }

        public DateTime Date { get; set; } // ngày viết nhật ký này
    }
}
