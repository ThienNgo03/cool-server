namespace Library.Models.PaginationParameters;

public class Model
{
    public string? Ids { get; set; }
    public int? PageIndex { get; set; }
    public int? PageSize { get; set; }
    public string? SearchTerm { get; set; }
    public string? Include { get; set; }
}
