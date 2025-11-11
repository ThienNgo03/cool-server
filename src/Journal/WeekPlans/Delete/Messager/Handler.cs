namespace Journal.WeekPlans.Delete.Messager;

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
            // Find the workout that has this WeekPlan
            var workout = await _mongoDbContext.Workouts
                .FirstOrDefaultAsync(w => w.Id == message.workoutId);

            if (workout == null)
            {
                Console.WriteLine($"Workout {message.workoutId} not found");
                return;
            }

            if (workout.WeekPlans == null || !workout.WeekPlans.Any())
            {
                Console.WriteLine($"No WeekPlans found in Workout {message.workoutId}");
                return;
            }

            // Remove the WeekPlan
            var initialCount = workout.WeekPlans.Count;
            workout.WeekPlans.RemoveAll(wp => wp.Id == message.parameters.Id);

            // Only update if something was removed
            if (workout.WeekPlans.Count < initialCount)
            {
                workout.LastUpdated = DateTime.UtcNow;
                _mongoDbContext.Workouts.Update(workout);
                await _mongoDbContext.SaveChangesAsync();

                Console.WriteLine($"Deleted WeekPlan {message.parameters.Id} from Workout {message.workoutId}");
            }
            else
            {
                Console.WriteLine($"WeekPlan {message.parameters.Id} not found in Workout {message.workoutId}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"MongoDB error: {ex.Message}");
            throw;
        }

        // ===== SYNC CONTEXT TABLES =====
        if (message.parameters.IsWeekPlanSetDelete)
        {
            // Delete all WeekPlanSets
            var weekPlanSets = _context.WeekPlanSets
                .Where(wps => wps.WeekPlanId == message.parameters.Id);
            _context.WeekPlanSets.RemoveRange(weekPlanSets);
            await _context.SaveChangesAsync();
            Console.WriteLine($"Deleted all WeekPlanSets for WeekPlan {message.parameters.Id}");
        }
    }
}