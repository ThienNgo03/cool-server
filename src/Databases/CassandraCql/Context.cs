using Cassandra.Data.Linq;
using Journal.ExerciseMuscles.CassandraTables.ByMuscleIds;

namespace Journal.Databases.CassandraCql;

public class Context(Cassandra.ISession session)
{
    private readonly Cassandra.ISession _session = session;

    public Table<ExerciseMuscles.CassandraTables.ByExerciseIds.Table> ExerciseMuscleByExerciseIds => new(_session);
    public Table<Table> ExerciseMuscleByMuscleIds => new(_session);

}
