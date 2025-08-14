using System.ComponentModel.DataAnnotations;

namespace Journal.WeekPlans.Post
{
    public class Payload
    {
        [Required]
        public Guid WorkoutId { get; set; }

        public string DateOfWeek { get; set; } = string.Empty;

        //public DayOfWeek DateOfWeek { get; set; }

        public DateTime Time { get; set; }

        public int Rep { get; set; }

        public TimeSpan HoldingTime { get; set; }

        public int Set { get; set; }

    }
}
