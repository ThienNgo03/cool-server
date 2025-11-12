using Journal.Databases.MongoDb;

namespace Journal.Workouts.Post.Messager;

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
        if (message.weekPlans is null)
            return;
        List<Databases.MongoDb.Collections.Workout.WeekPlan> mongoWeekPlans = new();
        foreach (var weekPlan in message.weekPlans)
        {
            var newWeekPlan = new WeekPlans.Table
            {
                Id = Guid.NewGuid(),
                WorkoutId = message.workout.Id,
                DateOfWeek = weekPlan.DateOfWeek,
                Time = weekPlan.Time,
                CreatedDate = DateTime.UtcNow,
                LastUpdated = DateTime.UtcNow
            };
            _context.WeekPlans.Add(newWeekPlan);
            var mongoWeekPlan = new Databases.MongoDb.Collections.Workout.WeekPlan()
            {
                Id = newWeekPlan.Id,
                WorkoutId = message.workout.Id,
                DateOfWeek = weekPlan.DateOfWeek,
                Time = weekPlan.Time,
                CreatedDate = newWeekPlan.CreatedDate,
                LastUpdated = newWeekPlan.LastUpdated
            };

            if (weekPlan.WeekPlanSets == null)
                continue;
            foreach (var weekPlanSet in weekPlan.WeekPlanSets)
            {
                var newWeekPlanSet = new WeekPlanSets.Table
                {
                    Id = Guid.NewGuid(),
                    WeekPlanId = newWeekPlan.Id,
                    Value = weekPlanSet.Value,
                    CreatedDate = DateTime.UtcNow,
                    LastUpdated = DateTime.UtcNow
                };
                _context.WeekPlanSets.Add(newWeekPlanSet);
                mongoWeekPlan.WeekPlanSets?.Add(new Databases.MongoDb.Collections.Workout.WeekPlanSet
                {
                    Id = newWeekPlanSet.Id,
                    WeekPlanId = newWeekPlan.Id,
                    Value = weekPlanSet.Value,
                    CreatedDate = newWeekPlanSet.CreatedDate,
                    LastUpdated = newWeekPlanSet.LastUpdated
                });
            }
            mongoWeekPlans.Add(mongoWeekPlan);
        }
        await _context.SaveChangesAsync();

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

        var workoutCollection = new Journal.Databases.MongoDb.Collections.Workout.Collection
        {
            Id = message.workout.Id,
            ExerciseId = message.workout.ExerciseId,
            UserId = message.workout.UserId,
            CreatedDate = message.workout.CreatedDate,
            LastUpdated = message.workout.LastUpdated,
            Exercise = exercise != null ? new Journal.Databases.MongoDb.Collections.Workout.Exercise
            {
                Id = exercise.Id,
                Name = exercise.Name,
                Description = exercise.Description,
                Type = exercise.Type,
                CreatedDate = exercise.CreatedDate,
                LastUpdated = exercise.LastUpdated,
                Muscles = musclesByExerciseId.GetValueOrDefault(exercise.Id, new List<Journal.Databases.MongoDb.Collections.Workout.Muscle>())
            } : null,
            WeekPlans = mongoWeekPlans
        };
        try
        {
            _mongoDbContext.Workouts.AddRange(workoutCollection);
            await _mongoDbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            return;
        }
    }
    #endregion
}
