namespace Library.Competitions.Delete;

public class Parameters
{
    public Guid Id { get; set; }
    public bool DeleteSoloPool { get; set; } = false;
    public bool DeleteTeamPool { get; set; } = false;
}
