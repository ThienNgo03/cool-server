using Cassandra.Data.Linq;
using Journal.ExerciseMuscles.Tables.CassandraTables.ByExerciseIds;
using Journal.ExerciseMuscles.Tables.CassandraTables.ByMuscleIds;

namespace Journal.Databases.CassandraCql;

public class Context(Cassandra.ISession session)
{
    private readonly Cassandra.ISession _session = session;

    public Table<ExerciseMuscles.Tables.CassandraTables.ByExerciseIds.Table> ExerciseMuscleByExerciseIds => new(_session);
    public Table<ExerciseMuscles.Tables.CassandraTables.ByMuscleIds.Table> ExerciseMuscleByMuscleIds => new(_session);

}
