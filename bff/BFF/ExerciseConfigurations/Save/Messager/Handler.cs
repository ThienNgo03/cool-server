using BFF.Databases.App;
using Microsoft.EntityFrameworkCore;

namespace BFF.ExerciseConfigurations.Save.Messager;

public class Handler
{
    #region [ Fields ]

    private readonly JournalDbContext _context;
    #endregion

    #region [ CTors ]

    public Handler(JournalDbContext context)
    {
        _context = context;
    }
    #endregion

    #region [ Handler ]

    public async Task Handle(Message message)
    {
        if (message.weekPlans is null)
            return;

        foreach (var weekPlan in message.weekPlans)
        {
            var newWeekPlan = new Databases.App.Tables.WeekPlan.Table
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
                var newWeekPlanSet = new Databases.App.Tables.WeekPlanSet.Table
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
        var oldWorkouts = await _context.Workouts
            .Where(w => w.ExerciseId == message.ExerciseId && w.UserId == message.UserId && w.Id != message.Id)
            .ToListAsync(); 
        foreach (var workout in oldWorkouts)
        {
            var oldWeekPlans = await _context.WeekPlans.Where(wp => wp.WorkoutId == workout.Id).ToListAsync();
            foreach (var weekPlan in oldWeekPlans)
            {
                var oldWeekPlanSets = _context.WeekPlanSets.Where(wps => wps.WeekPlanId == weekPlan.Id);
                _context.WeekPlanSets.RemoveRange(oldWeekPlanSets);
            }
            var oldWeekPlansToRemove = _context.WeekPlans.Where(wp => wp.WorkoutId == workout.Id);
            _context.WeekPlans.RemoveRange(oldWeekPlansToRemove);
        }
        _context.Workouts.RemoveRange(oldWorkouts);


        await _context.SaveChangesAsync();
    }
    #endregion
}
