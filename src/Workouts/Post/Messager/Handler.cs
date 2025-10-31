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

        foreach (var weekPlan in message.weekPlans)
        {
            var newWeekPlan = new WeekPlans.Table
            {
                Id = Guid.NewGuid(),
                WorkoutId = message.Id,
                DateOfWeek = weekPlan.DateOfWeek,
                Time = weekPlan.Time,
                CreatedDate = DateTime.UtcNow,
                LastUpdated = DateTime.UtcNow
            };
            _context.WeekPlans.Add(newWeekPlan);

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
            }
        }
        await _context.SaveChangesAsync();

        var workout = await _context.Workouts.FirstOrDefaultAsync(x => x.Id == message.Id);

        if (workout == null)
            return;

        var weekPlans = await _context.WeekPlans
            .Where(wp => wp.WorkoutId == workout.Id)
            .ToListAsync();

        var weekPlanIds = weekPlans.Select(wp => wp.Id).ToList();

        var weekPlanSets = await _context.WeekPlanSets
            .Where(wps => weekPlanIds.Contains(wps.WeekPlanId))
            .ToListAsync();

        var exerciseMuscles = await _context.ExerciseMuscles
            .Where(em => em.ExerciseId == workout.ExerciseId)
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

        var weekPlanSetsByWeekPlanId = weekPlanSets
            .GroupBy(wps => wps.WeekPlanId)
            .ToDictionary(
                g => g.Key,
                g => g.Select(wps => new Journal.Databases.MongoDb.Collections.Workout.WeekPlanSet
                {
                    Id = wps.Id,
                    Value = wps.Value,
                    WeekPlanId = wps.WeekPlanId,
                    InsertedBy = wps.InsertedBy,
                    UpdatedBy = wps.UpdatedBy,
                    LastUpdated = wps.LastUpdated ?? DateTime.MinValue,
                    CreatedDate = wps.CreatedDate
                }).ToList()
            );

        var weekPlansByWorkoutId = weekPlans
            .GroupBy(wp => wp.WorkoutId)
            .ToDictionary(
                g => g.Key,
                g => g.Select(wp => new Journal.Databases.MongoDb.Collections.Workout.WeekPlan
                {
                    Id = wp.Id,
                    DateOfWeek = wp.DateOfWeek,
                    Time = wp.Time,
                    WorkoutId = wp.WorkoutId,
                    CreatedDate = wp.CreatedDate,
                    LastUpdated = wp.LastUpdated,
                    WeekPlanSets = weekPlanSetsByWeekPlanId.GetValueOrDefault(wp.Id, new List<Journal.Databases.MongoDb.Collections.Workout.WeekPlanSet>())
                }).ToList()
            );

        var exercise = await _context.Exercises.FirstOrDefaultAsync(x => x.Id == workout.ExerciseId);

        var workoutCollection = new Journal.Databases.MongoDb.Collections.Workout.Collection
        {
            Id = workout.Id,
            ExerciseId = workout.ExerciseId,
            UserId = workout.UserId,
            CreatedDate = workout.CreatedDate,
            LastUpdated = workout.LastUpdated,
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
            WeekPlans = weekPlansByWorkoutId.GetValueOrDefault(workout.Id, new List<Journal.Databases.MongoDb.Collections.Workout.WeekPlan>())
        };

        _mongoDbContext.Workouts.AddRange(workoutCollection);
        await _mongoDbContext.SaveChangesAsync();
    }
    #endregion
}
