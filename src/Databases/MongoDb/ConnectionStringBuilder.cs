namespace Journal.Databases.MongoDb;

public class ConnectionStringBuilder
{
    private string _host;
    private int? _port;
    private string _database;
    private string _username;
    private string _password;
    private string _authDatabase = "admin";

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

    public ConnectionStringBuilder WithAuthDatabase(string authDatabase)
    {
        _authDatabase = authDatabase;
        return this;
    }

    public string Build()
    {
        var credentials = string.IsNullOrEmpty(_username) || string.IsNullOrEmpty(_password)
            ? ""
            : $"{_username}:{_password}@";

        var serverPart = _port.HasValue ? $"{_host}:{_port}" : _host;

        return $"mongodb://{credentials}{serverPart}/{_authDatabase}";
    }

    public string GetDatabaseName()
    {
        return _database;
    }
}