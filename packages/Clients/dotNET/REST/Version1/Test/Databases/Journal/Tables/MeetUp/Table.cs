using System.ComponentModel.DataAnnotations;

namespace Test.Databases.Journal.Tables.MeetUp;

public class Table
{
    [Required]
    public Guid Id { get; set; }
    [Required]
    public string ParticipantIds { get; set; }
    public string Title { get; set; }
    public DateTime DateTime { get; set; }
    public string Location { get; set; }
    public string CoverImage { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime LastUpdated { get; set; }


}
