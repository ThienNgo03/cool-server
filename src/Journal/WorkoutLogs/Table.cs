using System.ComponentModel.DataAnnotations.Schema;

namespace Journal.WorkoutLogs;

[Table("work-out-logs", Schema = "journal")]
public class Table : Model { }