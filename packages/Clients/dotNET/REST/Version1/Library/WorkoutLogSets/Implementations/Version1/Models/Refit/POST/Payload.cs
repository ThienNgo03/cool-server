

using System.ComponentModel.DataAnnotations;

namespace Library.WorkoutLogSets.Implementations.Version1.Models.Refit.POST;

public class Payload
{
    [Required]
    public Guid WorkoutLogId { get; set; }

    public int Value { get; set; }

}
