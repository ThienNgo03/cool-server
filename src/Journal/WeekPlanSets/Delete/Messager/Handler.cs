namespace Journal.WeekPlanSets.Delete.Messager;

using Journal.Databases;
using Journal.Databases.MongoDb;
using Microsoft.EntityFrameworkCore;

public class Handler
{
    private readonly JournalDbContext _context;
    private readonly MongoDbContext _mongoDbContext;

    public Handler(JournalDbContext context, MongoDbContext mongoDbContext)
    {
        _context = context;
        _mongoDbContext = mongoDbContext;
    }

    public async Task Handle(Message message)
    {
        // ===== SYNC MONGODB =====
        try
        {
            // Find the workout that has this WeekPlanSet
            var workout = await _mongoDbContext.Workouts
                .FirstOrDefaultAsync(w => w.WeekPlans != null &&
                    w.WeekPlans.Any(wp => wp.Id == message.weekPlanId));

            if (workout == null)
            {
                Console.WriteLine($"Workout with WeekPlan {message.weekPlanId} not found");
                return;
            }

            var weekPlan = workout.WeekPlans?.FirstOrDefault(wp => wp.Id == message.weekPlanId);
            if (weekPlan == null)
            {
                Console.WriteLine($"WeekPlan {message.weekPlanId} not found in workout");
                return;
            }

            if (weekPlan.WeekPlanSets == null || !weekPlan.WeekPlanSets.Any())
            {
                Console.WriteLine($"No WeekPlanSets found in WeekPlan {message.weekPlanId}");
                return;
            }

            // Remove the WeekPlanSet
            var initialCount = weekPlan.WeekPlanSets.Count;
            weekPlan.WeekPlanSets.RemoveAll(wps => wps.Id == message.weekPlanSetId);

            // Only update if something was removed
            if (weekPlan.WeekPlanSets.Count < initialCount)
            {
                weekPlan.LastUpdated = DateTime.UtcNow;
                workout.LastUpdated = DateTime.UtcNow;
                _mongoDbContext.Workouts.Update(workout);
                await _mongoDbContext.SaveChangesAsync();

                Console.WriteLine($"Deleted WeekPlanSet {message.weekPlanSetId} from WeekPlan {message.weekPlanId}");
            }
            else
            {
                Console.WriteLine($"WeekPlanSet {message.weekPlanSetId} not found in WeekPlan {message.weekPlanId}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"MongoDB error: {ex.Message}");
            throw;
        }
    }
}