namespace Journal.Workouts.Delete.Messager;

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
        List<Guid> weekPlanIds = new();

        var weekPlans = await _context.WeekPlans
            .Where(x => x.WorkoutId == message.Id)
            .ToListAsync();

        weekPlanIds = weekPlans.Select(w => w.Id).ToList();
        if (message.IsWeekPlanDelete)
        {
            _context.WeekPlans.RemoveRange(weekPlans);
        }
        if (message.IsWeekPlanSetDelete)
        {
            var weekPlanSets = await _context.WeekPlanSets
                .Where(x => weekPlanIds.Contains(x.WeekPlanId))
                .ToListAsync();

            _context.WeekPlanSets.RemoveRange(weekPlanSets);
        }
        await _context.SaveChangesAsync();
    }
    #endregion
}
