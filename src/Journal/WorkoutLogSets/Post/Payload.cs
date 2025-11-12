using System.ComponentModel.DataAnnotations;

namespace Journal.WorkoutLogSets.Post;

public class Payload
{
    [Required]
    public Guid WorkoutLogId { get; set; }

    public int Value { get; set; }

}
