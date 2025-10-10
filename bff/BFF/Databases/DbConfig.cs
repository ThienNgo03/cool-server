namespace BFF.Databases;

public class DbConfig
{
    public string Host { get; set; }=string.Empty;
    public int? Port { get; set; }
    public string Database { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string TrustedConnection { get; set; } = string.Empty;
    public string TrustServerCertificate { get; set; } = string.Empty;

}
