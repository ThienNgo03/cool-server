using System.ComponentModel.DataAnnotations;

namespace Journal.Competitions.Delete;

public class Parameters
{
    [Required]
    public Guid Id { get; set; }
    public bool DeleteSoloPool { get; set; }=false;
    public bool DeleteTeamPool { get; set; } = false;
}
