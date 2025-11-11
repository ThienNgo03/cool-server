namespace Journal.WeekPlans.Post.Messager;

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
        if (message.weekPlan == null)
            return;

        // ===== SYNC MONGODB =====
        try
        {
            var workout = await _mongoDbContext.Workouts
                .FirstOrDefaultAsync(w => w.Id == message.weekPlan.WorkoutId);

            if (workout == null)
            {
                Console.WriteLine($"Workout {message.weekPlan.WorkoutId} not found");
                return;
            }

            // Initialize WeekPlans list if null
            if (workout.WeekPlans == null)
            {
                workout.WeekPlans = new List<Databases.MongoDb.Collections.Workout.WeekPlan>();
            }

            // Create new WeekPlan for MongoDB
            var mongoWeekPlan = new Databases.MongoDb.Collections.Workout.WeekPlan
            {
                Id = message.weekPlan.Id,
                WorkoutId = message.weekPlan.WorkoutId,
                DateOfWeek = message.weekPlan.DateOfWeek,
                Time = message.weekPlan.Time,
                CreatedDate = message.weekPlan.CreatedDate,
                LastUpdated = message.weekPlan.LastUpdated,
                WeekPlanSets = new List<Databases.MongoDb.Collections.Workout.WeekPlanSet>()
            };

            workout.WeekPlans.Add(mongoWeekPlan);
            workout.LastUpdated = DateTime.UtcNow;

            _mongoDbContext.Workouts.Update(workout);
            await _mongoDbContext.SaveChangesAsync();

            Console.WriteLine($"Added WeekPlan {message.weekPlan.Id} to Workout {message.weekPlan.WorkoutId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"MongoDB error: {ex.Message}");
            throw;
        }
    }
}