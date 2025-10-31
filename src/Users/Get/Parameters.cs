namespace Journal.Users.Get;

public class Parameters : Models.PaginationParameters.Model
{
    public string? Ids { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public bool IsSelf { get; set; }
}
