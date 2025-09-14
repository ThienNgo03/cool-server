namespace Library.Files.Identifiers.Implementations.Version1.Models.Refit.GET;

public class Parameters
{
    public string? Id { get; set; }

    public string? Permissions { get; set; }
    public DateTimeOffset? PolicyStartsOn { get; set; }
    public DateTimeOffset? PolicyExpiresOn { get; set; }

    public int? PageIndex { get; set; }

    public int? PageSize { get; set; }
}
