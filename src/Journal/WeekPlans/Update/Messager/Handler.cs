namespace Journal.WeekPlans.Update.Messager;

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
            // Step 1: Find and remove the old WeekPlan from the old workout
            var oldWorkout = await _mongoDbContext.Workouts
                .FirstOrDefaultAsync(w => w.Id == message.oldWorkoutId);

            if (oldWorkout == null)
            {
                Console.WriteLine($"Old Workout {message.oldWorkoutId} not found");
                return;
            }

            // Step 1.1: Get the old WeekPlan to preserve WeekPlanSets
            var oldWeekPlan = oldWorkout.WeekPlans?.FirstOrDefault(wp => wp.Id == message.weekPlan.Id);
            List<Databases.MongoDb.Collections.Workout.WeekPlanSet>? existingWeekPlanSets = null;

            if (oldWeekPlan != null)
            {
                existingWeekPlanSets = oldWeekPlan.WeekPlanSets;
            }

            if (oldWorkout.WeekPlans != null)
            {
                var removed = oldWorkout.WeekPlans.RemoveAll(wp => wp.Id == message.weekPlan.Id);
                if (removed > 0)
                {
                    oldWorkout.LastUpdated = DateTime.UtcNow;
                }
            }

            // Step 2: Create new WeekPlan for MongoDB, keeping old WeekPlanSets
            var mongoWeekPlan = new Databases.MongoDb.Collections.Workout.WeekPlan
            {
                Id = message.weekPlan.Id,
                WorkoutId = message.weekPlan.WorkoutId,
                DateOfWeek = message.weekPlan.DateOfWeek,
                Time = message.weekPlan.Time,
                CreatedDate = message.weekPlan.CreatedDate,
                LastUpdated = message.weekPlan.LastUpdated,
                WeekPlanSets = existingWeekPlanSets ?? new List<Databases.MongoDb.Collections.Workout.WeekPlanSet>()
            };

            // Step 3: Add to the new workout (might be the same as old workout)
            var newWorkout = await _mongoDbContext.Workouts
                .FirstOrDefaultAsync(w => w.Id == message.weekPlan.WorkoutId);

            if (newWorkout == null)
            {
                Console.WriteLine($"New Workout {message.weekPlan.WorkoutId} not found");
                return;
            }

            // Initialize WeekPlans list if null
            if (newWorkout.WeekPlans == null)
            {
                newWorkout.WeekPlans = new List<Databases.MongoDb.Collections.Workout.WeekPlan>();
            }

            newWorkout.WeekPlans.Add(mongoWeekPlan);
            newWorkout.LastUpdated = DateTime.UtcNow;

            // Update both workouts (if they're different, EF will handle it)
            if (message.oldWorkoutId != message.weekPlan.WorkoutId)
            {
                _mongoDbContext.Workouts.UpdateRange(oldWorkout, newWorkout);
                Console.WriteLine($"Moved WeekPlan {message.weekPlan.Id} from Workout {message.oldWorkoutId} to Workout {message.weekPlan.WorkoutId}");
            }
            else
            {
                _mongoDbContext.Workouts.Update(newWorkout);
                Console.WriteLine($"Updated WeekPlan {message.weekPlan.Id} in Workout {message.weekPlan.WorkoutId}");
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