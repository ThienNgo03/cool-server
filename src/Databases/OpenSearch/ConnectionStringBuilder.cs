namespace Journal.Databases.OpenSearch;

public class ConnectionStringBuilder
{
    private string _host;
    private int _port = 9200;
    private string _username;
    private string _password;
    private bool _enableSsl = false;
    private bool _skipCertificateValidation = false;

    public ConnectionStringBuilder WithHost(string host)
    {
        _host = host;
        return this;
    }

    public ConnectionStringBuilder WithPort(int port)
    {
        _port = port;
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

    public ConnectionStringBuilder WithSsl()
    {
        _enableSsl = true;
        return this;
    }

    public ConnectionStringBuilder WithSkipCertificateValidation()
    {
        _skipCertificateValidation = true;
        return this;
    }

    public string Build()
    {
        var scheme = _enableSsl ? "https" : "http";
        return $"{scheme}://{_host}:{_port}";
    }

    public string GetBasicAuthHeader()
    {
        if (string.IsNullOrEmpty(_username) || string.IsNullOrEmpty(_password))
            return null;

        var credentials = $"{_username}:{_password}";
        var bytes = System.Text.Encoding.ASCII.GetBytes(credentials);
        return Convert.ToBase64String(bytes);
    }

    public bool ShouldSkipCertificateValidation()
    {
        return _skipCertificateValidation;
    }
}
