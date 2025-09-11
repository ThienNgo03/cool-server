
namespace Library.Users.Implementations.Version1.Models.Refit.GET;

public class Parameters
{
    public Guid? Id { get; set; }
    public int? PageIndex { get; set; }
    public int? PageSize { get; set; }
    public string? SearchTerm { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public bool IsSelf { get; set; }
}
