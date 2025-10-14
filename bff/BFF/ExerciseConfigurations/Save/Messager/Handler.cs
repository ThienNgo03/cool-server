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
        //Add new week plans
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

        //Select old workouts
        
        ICollection<Guid> oldWorkoutIds = [];
        if(message.OldWorkoutId.Count>0)
        {
            oldWorkoutIds = message.OldWorkoutId;
        }
        var oldWeekPlans = await _context.WeekPlans.Where(wp => oldWorkoutIds.Contains(wp.Id)).ToListAsync();
        var oldWeekPlanIds = oldWeekPlans.Select(o => o.Id).ToList();
        
        _context.WeekPlanSets.Where(wps =>oldWeekPlanIds.Contains(wps.WeekPlanId)).ExecuteDelete();
        _context.WeekPlans.Where(wp => oldWorkoutIds.Contains(wp.WorkoutId)).ExecuteDelete();      
        _context.Workouts
            .Where(w => w.ExerciseId == message.ExerciseId && w.UserId == message.UserId && w.Id != message.Id)
            .ExecuteDelete();

        //Update existing Workout log related to the old workout to the new workout Id
        _context.WorkoutLogs.Where(x=>oldWorkoutIds.Contains(x.WorkoutId))
            .ExecuteUpdate(s=>s.SetProperty(p=>p.WorkoutId,p=>message.Id)
                                .SetProperty(p=>p.LastUpdated,p=>DateTime.UtcNow));

        await _context.SaveChangesAsync();
    }
    #endregion
}
