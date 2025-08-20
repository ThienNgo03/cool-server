using System.ComponentModel.DataAnnotations;

namespace Journal.ExerciseMuscles.Update;

public class Payload
{
    public Guid Id { get; set; }

    public Guid ExerciseId { get; set; }

    public Guid MuscleId { get; set; }

}
