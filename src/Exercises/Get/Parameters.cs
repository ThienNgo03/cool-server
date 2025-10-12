using Microsoft.Identity.Client;

namespace Journal.Exercises.Get;

public class Parameters : Models.PaginationParameters.Model
{
    public string? Ids { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Type { get; set; }
    public string? MusclesSortBy { get; set; }
    public string? MusclesSortOrder { get; set; }

    public string? SearchTerm { get; set; }
}
