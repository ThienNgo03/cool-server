namespace Library.Models;

public class ApiResponse<T>
{
    public int Total { get; set; }
    public int? PageIndex { get; set; }
    public int? PageSize { get; set; }
    public List<T> Data { get; set; } = new List<T>();
}
