namespace Library.Files.Identifiers.Implementations.Version1.Models.Refit.GET;

public class Data
{
    public string Id { get; set; }

    public BlobAccessPolicy AccessPolicy { get; set; }
}

public class BlobAccessPolicy
{
    public string Permissions { get; set; }
    public DateTimeOffset PolicyStartsOn { get; set; }
    public DateTimeOffset PolicyExpiresOn { get; set; }
}
