using Journal.Databases.MongoDb;

namespace Journal.Workouts.Update.Messager;

public class Handler
{
    #region [ Fields ]

    private readonly JournalDbContext _context;
    private readonly MongoDbContext _mongoDbContext;
    #endregion

    #region [ CTors ]

    public Handler(JournalDbContext context, MongoDbContext mongoDbContext)
    {
        _context = context;
        _mongoDbContext = mongoDbContext;
    }
    #endregion

    #region [ Handler ]

    public async Task Handle(Message message)
    {
        var exerciseMuscles = await _context.ExerciseMuscles
            .Where(em => em.ExerciseId == message.workout.ExerciseId)
            .ToListAsync();

        var muscleIds = exerciseMuscles.Select(em => em.MuscleId).Distinct().ToList();
        var muscles = await _context.Muscles
            .Where(m => muscleIds.Contains(m.Id))
            .ToDictionaryAsync(m => m.Id);

        var musclesByExerciseId = exerciseMuscles
            .GroupBy(em => em.ExerciseId)
            .ToDictionary(
                g => g.Key,
                g => g.Where(em => muscles.ContainsKey(em.MuscleId))
                        .Select(em => new Journal.Databases.MongoDb.Collections.Workout.Muscle
                        {
                            Id = muscles[em.MuscleId].Id,
                            Name = muscles[em.MuscleId].Name,
                            CreatedDate = muscles[em.MuscleId].CreatedDate,
                            LastUpdated = muscles[em.MuscleId].LastUpdated
                        }).ToList()
            );

        var exercise = await _context.Exercises.FirstOrDefaultAsync(x => x.Id == message.workout.ExerciseId);
        try
        {
            var mongoWorkout = await _mongoDbContext.Workouts.FirstOrDefaultAsync(x => x.Id == message.workout.Id);

            if (mongoWorkout == null)
                return;

            mongoWorkout.ExerciseId = message.workout.ExerciseId;
            mongoWorkout.UserId = message.workout.UserId;
            mongoWorkout.LastUpdated = message.workout.LastUpdated;
            mongoWorkout.Exercise = exercise != null ? new Journal.Databases.MongoDb.Collections.Workout.Exercise
            {
                Id = exercise.Id,
                Name = exercise.Name,
                Description = exercise.Description,
                Type = exercise.Type,
                CreatedDate = exercise.CreatedDate,
                LastUpdated = exercise.LastUpdated,
                Muscles = musclesByExerciseId.GetValueOrDefault(exercise.Id, new List<Journal.Databases.MongoDb.Collections.Workout.Muscle>())
            } : null;

            _mongoDbContext.Workouts.Update(mongoWorkout);
            await _mongoDbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            return;
        }
    }
    #endregion
}