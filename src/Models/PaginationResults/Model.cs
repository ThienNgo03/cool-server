namespace Journal.Models.PaginationResults;

public class Model<T>
{
    public int All { get; set; } = 0;
    public int Total { get; set; } = 0;
    public int? Index { get; set; }
    public int? Size { get; set; }
    public ICollection<T>? Items { get; set; }
}
