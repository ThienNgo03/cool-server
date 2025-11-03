namespace Library.Exercises.GET;

public class Parameters : Models.PaginationParameters.Model
{
    public string? Name { get; set; }

    public string? Description { get; set; }

    public string? Type { get; set; }

    public string? MusclesSortBy { get; set; }
    public string? MusclesSortOrder { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? LastUpdated { get; set; }
}
