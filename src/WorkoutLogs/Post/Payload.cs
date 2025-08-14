using System.ComponentModel.DataAnnotations;

namespace Journal.WorkoutLogs.Post
{
    public class Payload
    {
        [Required]
        public Guid WorkoutId { get; set; }

        public int Rep { get; set; }

        public TimeSpan HoldingTime { get; set; }

        public int Set { get; set; }

        public DateTime WorkoutDate { get; set; }

    }
}
