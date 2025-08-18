using System.ComponentModel.DataAnnotations;

namespace Journal.WorkoutLogSets.Update
{
    public class Payload
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        public Guid WorkoutLogId { get; set; }

        public int Value { get; set; }

    }
}
