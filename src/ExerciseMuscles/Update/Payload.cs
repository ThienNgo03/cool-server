using System.ComponentModel.DataAnnotations;

namespace Journal.ExerciseMuscles.Update;

public class Payload
{
    public Guid PartitionKey { get; set; }

    public Guid Id { get; set; }

    public Guid NewExerciseId { get; set; }

    public Guid NewMuscleId { get; set; }

}
