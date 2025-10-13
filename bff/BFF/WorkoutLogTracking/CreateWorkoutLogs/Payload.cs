using System.ComponentModel.DataAnnotations;

namespace BFF.WorkoutLogTracking.CreateWorkoutLogs;

public class Payload
{
    [Required]
    public Guid WorkoutId { get; set; }

    public DateTime WorkoutDate { get; set; }

}
