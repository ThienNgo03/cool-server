using Cassandra.Data.Linq;

namespace Test.Databases.CassandraCql;

public class Context(Cassandra.ISession session)
{
    private readonly Cassandra.ISession _session = session;

    public Table<Tables.ExerciseMuscle.Table> ExerciseMuscles => new(_session);

}
