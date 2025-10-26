using Cassandra.Mapping.Attributes;

namespace Journal.Databases.Messages.Tables.ExerciseMuscle;
[Table("exercise_muscle")]
public class Table
{
    public Guid Id { get; set; }

    public Guid ExerciseId { get; set; }

    public Guid MuscleId { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime LastUpdated { get; set; }
}
