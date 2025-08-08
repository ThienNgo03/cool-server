namespace Library.Models;

public class ApiResponse<T>
{
    public int Total { get; set; }
    public int? Index { get; set; }
    public int? Size { get; set; }
    public List<T> Items { get; set; } = new List<T>();
}
