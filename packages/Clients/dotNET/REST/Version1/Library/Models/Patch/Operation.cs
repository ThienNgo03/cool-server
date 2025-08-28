namespace Library.Models.Patch;

public class Operation
{
    public string Path { get; set; } = string.Empty;
    public object? Value { get; set; }
}
