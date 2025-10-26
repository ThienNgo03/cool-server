namespace Journal.Databases;

public class CassandraContactPointBuilder
{
    private string? _contactPoint;
    private int? _port;
    private string? _keyspace;
    public CassandraContactPointBuilder WithContactPoint(string contactPoint)
    {
        _contactPoint = contactPoint;
        return this;
    }

    public CassandraContactPointBuilder WithPort(int? port)
    {
        _port = port;
        return this;
    }

    public CassandraContactPointBuilder WithKeyspace(string keyspace)
    {
        _keyspace = keyspace;
        return this;
    }


    public string Build()
    {
        return $"ContactPoint={_contactPoint};" +
               $"Port:{_port}" +
               $"Namespace={_keyspace};";
    }
}
