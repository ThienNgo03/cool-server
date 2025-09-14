namespace Library.Files.Identifiers.All;

public class Parameters
{
    public string? Id { get; set; }

    public string? Permissions { get; set; }
    public DateTimeOffset? PolicyStartsOn { get; set; }
    public DateTimeOffset? PolicyExpiresOn { get; set; }

    public int? PageIndex { get; set; }

    public int? PageSize { get; set; }
}
