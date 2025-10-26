using Cassandra.Data.Linq;

namespace Journal.Databases.Messages;

public class Context(Cassandra.ISession session)
{
    private readonly Cassandra.ISession _session = session;

    public Table<Table> Messages => new(_session);

}
