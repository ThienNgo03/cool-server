namespace Journal.WeekPlanSets.Post.Messager;

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
            // Find the workout that contains the WeekPlan
            var workout = await _mongoDbContext.Workouts
                .FirstOrDefaultAsync(w => w.WeekPlans != null &&
                    w.WeekPlans.Any(wp => wp.Id == message.weekPlanSet.WeekPlanId));

            if (workout == null)
            {
                Console.WriteLine($"Workout with WeekPlan {message.weekPlanSet.WeekPlanId} not found");
                return;
            }

            var weekPlan = workout.WeekPlans?.FirstOrDefault(wp => wp.Id == message.weekPlanSet.WeekPlanId);
            if (weekPlan == null)
            {
                Console.WriteLine($"WeekPlan {message.weekPlanSet.WeekPlanId} not found in workout");
                return;
            }

            // Initialize WeekPlanSets list if null
            if (weekPlan.WeekPlanSets == null)
            {
                weekPlan.WeekPlanSets = new List<Databases.MongoDb.Collections.Workout.WeekPlanSet>();
            }

            // Create new WeekPlanSet for MongoDB
            var mongoWeekPlanSet = new Databases.MongoDb.Collections.Workout.WeekPlanSet
            {
                Id = message.weekPlanSet.Id,
                WeekPlanId = message.weekPlanSet.WeekPlanId,
                Value = message.weekPlanSet.Value,
                CreatedDate = message.weekPlanSet.CreatedDate,
                LastUpdated = message.weekPlanSet.LastUpdated
            };

            weekPlan.WeekPlanSets.Add(mongoWeekPlanSet);
            weekPlan.LastUpdated = DateTime.UtcNow;
            workout.LastUpdated = DateTime.UtcNow;

            _mongoDbContext.Workouts.Update(workout);
            await _mongoDbContext.SaveChangesAsync();

            Console.WriteLine($"Added WeekPlanSet {message.weekPlanSet.Id} to WeekPlan {message.weekPlanSet.WeekPlanId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"MongoDB error: {ex.Message}");
            throw;
        }
    }
}