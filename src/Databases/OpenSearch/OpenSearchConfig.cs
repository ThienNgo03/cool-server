namespace Journal.Databases;

public class OpenSearchConfig
{
    public string Host { get; set; }
    public int Port { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public bool EnableSsl { get; set; }
    public bool SkipCertificateValidation { get; set; }
}