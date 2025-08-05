using System.ComponentModel.DataAnnotations;

namespace Journal.MeetUps.Update
{
    public class Payload
    {
        [Required]
        public Guid Id { get; set; }
        [Required]
        public string ParticipantIds { get; set; }
        public string Title { get; set; }
        public DateTime DateTime { get; set; }
        public string Location { get; set; }
        public string CoverImage { get; set; }
    }
}
