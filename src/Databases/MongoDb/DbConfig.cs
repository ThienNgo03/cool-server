namespace Journal.Databases.MongoDb;

public class DbConfig
{
    public string Host { get; set; }
    public int? Port { get; set; }
    public string Database { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public string AuthDatabase { get; set; } = "admin";
}
