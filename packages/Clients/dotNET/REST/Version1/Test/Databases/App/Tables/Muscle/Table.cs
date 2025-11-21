using System.ComponentModel.DataAnnotations.Schema;

namespace Test.Databases.App.Tables.Muscle;

[Table("muscles", Schema = "journal")]
public class Table
{
    public Guid Id { get; set; }

    public string Name { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime LastUpdated { get; set; }
}
