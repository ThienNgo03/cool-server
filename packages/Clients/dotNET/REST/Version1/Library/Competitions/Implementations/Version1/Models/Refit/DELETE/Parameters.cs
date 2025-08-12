namespace Library.Competitions.Implementations.Version1.Models.Refit.DELETE;

public class Parameters
{
    public Guid Id { get; set; }
    public bool DeleteSoloPool { get; set; } = false;
    public bool DeleteTeamPool { get; set; } = false;
}
