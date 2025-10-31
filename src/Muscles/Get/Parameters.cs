namespace Journal.Muscles.Get;

public class Parameters : Models.PaginationParameters.Model
{
    public string? Ids { get; set; }
    public string? Name { get; set; }
}
