namespace Journal.Models.PaginationParameters;

public class Model
{
    public Guid? Id { get; set; }
    public int? PageIndex { get; set; }
    public int? PageSize { get; set; }
    public string? SearchTerm { get; set; }
    public DateTime? CreatedDate { get; set; }
    public DateTime? LastUpdated { get; set; }
}
