using System.ComponentModel.DataAnnotations;

namespace Journal.WorkoutLogs.Post
{
    public class Payload
    {
        [Required]
        public Guid WorkoutId { get; set; }

        public DateTime WorkoutDate { get; set; }

    }
}
