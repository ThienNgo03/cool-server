using System.ComponentModel.DataAnnotations.Schema;

namespace Test.Databases.App.Tables.Exercise;

[Table("exercises", Schema = "journal")]
public class Table
{
    public Guid Id { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public string Type { get; set; }

    public DateTime CreatedDate { get; set; }
    public Guid CreatedById { get; set; }
    public DateTime? LastUpdated { get; set; }
    public Guid? UpdatedById { get; set; }

}
