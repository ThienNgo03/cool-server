using Cassandra.Data.Linq;

namespace Journal.Databases.CassandraCql;

public class Context(Cassandra.ISession session)
{
    private readonly Cassandra.ISession _session = session;

    public Table<ExerciseMuscles.Table> ExerciseMuscles => new(_session);

}
