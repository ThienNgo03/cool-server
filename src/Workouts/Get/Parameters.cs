namespace Journal.Workouts.Get;

public class Parameters
{
    public string? Ids { get; set; }

    public Guid? ExerciseId { get; set; }

    public Guid? UserId { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? LastUpdated { get; set; }

    public int? PageSize { get; set; }

    public int? PageIndex { get; set; }

    public string? Include { get; set; }

    public string? SortBy { get; set; }

    public string? SortOrder { get; set; }
}
