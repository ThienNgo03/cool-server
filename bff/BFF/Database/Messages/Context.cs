using Cassandra.Data.Linq;

namespace BFF.Database.Messages;

public class Context
{
    private readonly Cassandra.ISession _session;
    public Context(Cassandra.ISession session)
    {
        _session = session;
    }
    public Table<Table> Messages => new(_session);

}
