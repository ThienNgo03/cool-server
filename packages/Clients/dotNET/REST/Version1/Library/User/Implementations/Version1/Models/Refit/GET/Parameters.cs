
namespace Library.User.Implementations.Version1.Models.Refit.GET;

public class Parameters
{
    public Guid? id { get; set; }
    public string? name { get; set; }
    public string? email { get; set; }
    public string? phoneNumber { get; set; }
    public int? PageIndex { get; set; }
    public int? PageSize { get; set; }
}
