using Journal.Databases.MongoDb;
using Microsoft.EntityFrameworkCore;

namespace Journal.Workouts.Patch.Messager;

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
        try
        {
            var mongoWorkout = await _mongoDbContext.Workouts.FirstOrDefaultAsync(x => x.Id == message.entity.Id);
            if (mongoWorkout == null)
                return;

            foreach (var (path, value) in message.changes)
            {
                var fieldName = path.TrimStart('/');

                // Check if ExerciseId was changed
                if (fieldName.ToLowerInvariant() == "exerciseid")
                {
                    if (value != null && Guid.TryParse(value.ToString(), out Guid newExerciseId)) {
                        mongoWorkout.ExerciseId = newExerciseId;

                // Get exercise muscles
                        var exerciseMuscles = await _context.ExerciseMuscles
                            .Where(em => em.ExerciseId == newExerciseId)
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

                        var exercise = await _context.Exercises.FirstOrDefaultAsync(x => x.Id == newExerciseId);

                        mongoWorkout.ExerciseId = newExerciseId;
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
                    }
                }

                // Apply other field changes (if any)

                if (fieldName.ToLowerInvariant() == "userid")
                {
                    if (value != null && Guid.TryParse(value.ToString(), out Guid userId))
                        mongoWorkout.UserId = userId;
                }

            }

            // Always update lastUpdated
            mongoWorkout.LastUpdated = message.entity.LastUpdated;

            _mongoDbContext.Workouts.Update(mongoWorkout);
            await _mongoDbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error patching workout in MongoDB: {ex.Message}");
            return;
        }
    }
    #endregion
}