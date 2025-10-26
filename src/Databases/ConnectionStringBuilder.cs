namespace Journal.Databases;

public class ConnectionStringBuilder
{
    private string _host;
    private int? _port;
    private string _database;
    private string _username;
    private string _password;
    private string _trustedConnection = "false";
    private string _trustServerCertificate = "false";

    public ConnectionStringBuilder WithHost(string host)
    {
        _host = host;
        return this;
    }

    public ConnectionStringBuilder WithPort(int? port)
    {
        _port = port;
        return this;
    }

    public ConnectionStringBuilder WithDatabase(string database)
    {
        _database = database;
        return this;
    }

    public ConnectionStringBuilder WithUsername(string username)
    {
        _username = username;
        return this;
    }

    public ConnectionStringBuilder WithPassword(string password)
    {
        _password = password;
        return this;
    }

    public ConnectionStringBuilder WithTrustedConnection()
    {
        _trustedConnection = "true";
        return this;
    }

    public ConnectionStringBuilder WithTrustServerCertificate()
    {
        _trustServerCertificate = "true";
        return this;
    }

    public string Build()
    {
        var serverPart = _port.HasValue ? $"{_host},{_port}" : _host;
        return $"Server={serverPart};" +
               $"Database={_database};" +
               $"User Id={_username};" +
               $"Password={_password};" +
               $"Trusted_Connection={_trustedConnection};" +
               $"TrustServerCertificate={_trustServerCertificate};";
    }
}