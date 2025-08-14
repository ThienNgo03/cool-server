using System.ComponentModel.DataAnnotations;

namespace Journal.Workouts.Post
{
    public class Payload
    {
        [Required]
        public Guid ExerciseId { get; set; }

        [Required]
        public Guid UserId { get; set; }

    }
}
