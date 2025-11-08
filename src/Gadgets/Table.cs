using System.ComponentModel.DataAnnotations.Schema;

namespace Journal.Gadgets;

[Table("gadgets", Schema = "journal")]
public class Table : Model { }