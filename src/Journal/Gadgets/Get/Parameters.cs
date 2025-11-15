namespace Journal.Gadgets.Get;

public class Parameters: Models.PaginationParameters.Model
{
    public string? Name { get; set; } 
    public string? Brand { get; set; } 
    public string? Description { get; set; } 
    public DateTime? Date { get; set; }
}