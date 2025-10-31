using System.ComponentModel.DataAnnotations;

namespace Library.WorkoutLogSets.POST;

public class Payload
{
    [Required]
    public Guid WorkoutLogId { get; set; }

    public int Value { get; set; }

}
