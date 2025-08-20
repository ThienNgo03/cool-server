namespace Journal.Workouts.Post.Messager;

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
            var newWeekPlan = new Databases.Journal.Tables.WeekPlan.Table
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
                var newWeekPlanSet = new Databases.Journal.Tables.WeekPlanSet.Table
                {
                    Id = Guid.NewGuid(),
                    WeekPlanId = newWeekPlan.Id,
                    Value = weekPlanSet.Value,
                    CreateAt = DateTime.UtcNow,
                    LastUpdated = DateTime.UtcNow
                };
                _context.WeekPlanSets.Add(newWeekPlanSet);
            }
        }
        await _context.SaveChangesAsync();
    }
    #endregion
}
