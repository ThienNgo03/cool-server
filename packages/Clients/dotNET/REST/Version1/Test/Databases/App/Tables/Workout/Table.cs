using System.ComponentModel.DataAnnotations.Schema;

namespace Test.Databases.App.Tables.Workout;

[Table("workouts", Schema = "journal")]
public class Table
{
    public Guid Id { get; set; }

    public Guid ExerciseId { get; set; }

    public Guid UserId { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime LastUpdated { get; set; }

}
