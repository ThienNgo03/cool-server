namespace Journal.WeekPlanSets.Update.Messager;

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
        if (message.weekPlanSet == null)
            return;

        // ===== SYNC MONGODB =====
        try
        {
            // Step 1: Find and remove the old WeekPlanSet from the old WeekPlan
            var oldWorkout = await _mongoDbContext.Workouts
                .FirstOrDefaultAsync(w => w.WeekPlans != null &&
                    w.WeekPlans.Any(wp => wp.Id == message.oldWeekPlanId));

            if (oldWorkout == null)
            {
                Console.WriteLine($"Workout with old WeekPlan {message.oldWeekPlanId} not found");
                return;
            }

            var oldWeekPlan = oldWorkout.WeekPlans?.FirstOrDefault(wp => wp.Id == message.oldWeekPlanId);
            if (oldWeekPlan == null)
            {
                Console.WriteLine($"Old WeekPlan {message.oldWeekPlanId} not found in workout");
                return;
            }

            if (oldWeekPlan.WeekPlanSets != null)
            {
                var removed = oldWeekPlan.WeekPlanSets.RemoveAll(wps => wps.Id == message.weekPlanSet.Id);
                if (removed > 0)
                {
                    oldWeekPlan.LastUpdated = DateTime.UtcNow;
                }
            }

            // Step 2: Create new WeekPlanSet for MongoDB
            var mongoWeekPlanSet = new Databases.MongoDb.Collections.Workout.WeekPlanSet
            {
                Id = message.weekPlanSet.Id,
                WeekPlanId = message.weekPlanSet.WeekPlanId,
                Value = message.weekPlanSet.Value,
                CreatedDate = message.weekPlanSet.CreatedDate,
                LastUpdated = message.weekPlanSet.LastUpdated
            };

            // Step 3: Add to the new WeekPlan (might be the same as old WeekPlan)
            var newWorkout = await _mongoDbContext.Workouts
                .FirstOrDefaultAsync(w => w.WeekPlans != null &&
                    w.WeekPlans.Any(wp => wp.Id == message.weekPlanSet.WeekPlanId));

            if (newWorkout == null)
            {
                Console.WriteLine($"Workout with new WeekPlan {message.weekPlanSet.WeekPlanId} not found");
                return;
            }

            var newWeekPlan = newWorkout.WeekPlans?.FirstOrDefault(wp => wp.Id == message.weekPlanSet.WeekPlanId);
            if (newWeekPlan == null)
            {
                Console.WriteLine($"New WeekPlan {message.weekPlanSet.WeekPlanId} not found in workout");
                return;
            }

            // Initialize WeekPlanSets list if null
            if (newWeekPlan.WeekPlanSets == null)
            {
                newWeekPlan.WeekPlanSets = new List<Databases.MongoDb.Collections.Workout.WeekPlanSet>();
            }

            newWeekPlan.WeekPlanSets.Add(mongoWeekPlanSet);
            newWeekPlan.LastUpdated = DateTime.UtcNow;
            newWorkout.LastUpdated = DateTime.UtcNow;

            // Update workouts (if they're different, EF will handle it)
            if (oldWorkout.Id != newWorkout.Id)
            {
                _mongoDbContext.Workouts.UpdateRange(oldWorkout, newWorkout);
                Console.WriteLine($"Moved WeekPlanSet {message.weekPlanSet.Id} from WeekPlan {message.oldWeekPlanId} to WeekPlan {message.weekPlanSet.WeekPlanId}");
            }
            else
            {
                _mongoDbContext.Workouts.Update(newWorkout);
                Console.WriteLine($"Updated WeekPlanSet {message.weekPlanSet.Id} in WeekPlan {message.weekPlanSet.WeekPlanId}");
            }

            await _mongoDbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"MongoDB error: {ex.Message}");
            throw;
        }
    }
}