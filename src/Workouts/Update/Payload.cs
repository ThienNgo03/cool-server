using System.ComponentModel.DataAnnotations;

namespace Journal.Workouts.Update
{
    public class Payload
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        public Guid ExerciseId { get; set; }

        [Required]
        public Guid UserId { get; set; }

    }
}
