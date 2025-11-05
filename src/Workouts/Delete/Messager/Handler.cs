using Journal.Databases.MongoDb;

namespace Journal.Workouts.Delete.Messager;

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
        List<Guid> weekPlanIds = new();
        var weekPlans = await _context.WeekPlans
            .Where(x => x.WorkoutId == message.Id)
            .ToListAsync();
        weekPlanIds = weekPlans.Select(w => w.Id).ToList();

        // Delete WeekPlans if requested
        if (message.IsWeekPlanDelete)
        {
            _context.WeekPlans.RemoveRange(weekPlans);
            Console.WriteLine($"Deleted {weekPlans.Count} WeekPlan(s) for Workout {message.Id}");
        }

        // Delete WeekPlanSets if requested
        if (message.IsWeekPlanSetDelete)
        {
            var weekPlanSets = await _context.WeekPlanSets
                .Where(x => weekPlanIds.Contains(x.WeekPlanId))
                .ToListAsync();
            _context.WeekPlanSets.RemoveRange(weekPlanSets);
            Console.WriteLine($"Deleted {weekPlanSets.Count} WeekPlanSet(s) for Workout {message.Id}");
        }

        await _context.SaveChangesAsync();

        // ===== SYNC MONGODB =====
        try
        {
            var mongoWorkout = await _mongoDbContext.Workouts.FirstOrDefaultAsync(x => x.Id == message.Id);
            if (mongoWorkout == null)
            {
                Console.WriteLine($"Workout {message.Id} not found in MongoDB");
                return;
            }

            _mongoDbContext.Workouts.Remove(mongoWorkout);
            await _mongoDbContext.SaveChangesAsync();
            Console.WriteLine($"Deleted Workout {message.Id} from MongoDB");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"MongoDB error: {ex.Message}");
            return;
        }
    }
    #endregion
}