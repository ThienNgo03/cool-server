using System.ComponentModel.DataAnnotations.Schema;

namespace Journal.Notes;

[Table("notes", Schema = "journal")]
public class Table : Model { }