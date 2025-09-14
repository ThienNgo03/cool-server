namespace Library.Files.Identifiers.Update;

public class Payload
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
