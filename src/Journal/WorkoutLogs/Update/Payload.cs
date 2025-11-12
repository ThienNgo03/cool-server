using System.ComponentModel.DataAnnotations;

namespace Journal.WorkoutLogs.Update
{
    public class Payload
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        public Guid WorkoutId { get; set; }

        public DateTime WorkoutDate { get; set; }

    }
}
