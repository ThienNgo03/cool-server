using System.ComponentModel.DataAnnotations;

namespace Journal.WeekPlans.Update
{
    public class Payload
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        public Guid WorkoutId { get; set; }

        public string DateOfWeek { get; set; } = string.Empty;

        public TimeSpan Time { get; set; }

    }
}
