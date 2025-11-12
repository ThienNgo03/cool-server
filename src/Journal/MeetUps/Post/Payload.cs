using System.ComponentModel.DataAnnotations;

namespace Journal.MeetUps.Post
{
    public class Payload
    {
        [Required]
        public string ParticipantIds { get; set; }

        public string Title { get; set; } = string.Empty;

        public DateTime DateTime { get; set; }

        public string Location { get; set; } = string.Empty;

        public string CoverImage { get; set; } = string.Empty;
    }
}
