using System.ComponentModel.DataAnnotations;

namespace Journal.WeekPlans.Post
{
    public class Payload
    {
        [Required]
        public Guid WorkoutId { get; set; }

        public string DateOfWeek { get; set; } = string.Empty;

        public DateTime Time { get; set; }

    }
}
