
using System.ComponentModel.DataAnnotations;

namespace Library.WorkoutLogSets.Implementations.Version1.Models.Refit.PUT;

public class Payload
{
    [Required]
    public Guid Id { get; set; }

    [Required]
    public Guid WorkoutLogId { get; set; }

    public int Value { get; set; }

}
