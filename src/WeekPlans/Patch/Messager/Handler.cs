namespace Journal.WeekPlans.Patch.Messager;

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
        if (message.changes == null || !message.changes.Any())
            return;

        // Get the updated weekPlan from database
        var updatedWeekPlan = await _context.WeekPlans.FirstOrDefaultAsync(wp => wp.Id == message.weekPlanId);
        if (updatedWeekPlan == null)
        {
            Console.WriteLine($"WeekPlan {message.weekPlanId} not found.");
            return;
        }

        // ===== SYNC MONGODB =====
        try
        {
            // Find the current workout that has this WeekPlan
            var currentWorkout = await _mongoDbContext.Workouts
                .FirstOrDefaultAsync(w => w.WeekPlans != null && w.WeekPlans.Any(wp => wp.Id == message.weekPlanId));

            if (currentWorkout == null)
            {
                Console.WriteLine($"Workout with WeekPlan {message.weekPlanId} not found");
                return;
            }

            var weekPlanToUpdate = currentWorkout.WeekPlans?.FirstOrDefault(wp => wp.Id == message.weekPlanId);
            if (weekPlanToUpdate == null)
            {
                Console.WriteLine($"WeekPlan {message.weekPlanId} not found in workout");
                return;
            }

            // Check if WorkoutId is being changed
            bool workoutIdChanged = false;
            Guid? newWorkoutId = null;

            foreach (var (path, value) in message.changes)
            {
                var fieldName = path.TrimStart('/').ToLowerInvariant();

                if (fieldName == "workoutid")
                {
                    if (value is string guidString && Guid.TryParse(guidString, out Guid guidValue))
                    {
                        newWorkoutId = guidValue;
                        workoutIdChanged = currentWorkout.Id != guidValue;
                    }
                    continue; // Don't apply WorkoutId change yet
                }

                // Apply other changes
                switch (fieldName)
                {
                    case "dateofweek":
                        if (value is string stringValue)
                            weekPlanToUpdate.DateOfWeek = stringValue;
                        break;
                    case "time":
                        if (value is TimeSpan timeSpanValue)
                            weekPlanToUpdate.Time = timeSpanValue;
                        break;
                        // Add other patchable fields as needed
                }
            }

            // Always update lastUpdated
            weekPlanToUpdate.LastUpdated = updatedWeekPlan.LastUpdated;

            // If WorkoutId changed, move the WeekPlan to the new workout
            if (workoutIdChanged && newWorkoutId.HasValue)
            {
                // Preserve the WeekPlanSets before removing
                var weekPlanSetsToKeep = weekPlanToUpdate.WeekPlanSets;

                // Remove from current workout
                currentWorkout.WeekPlans?.RemoveAll(wp => wp.Id == message.weekPlanId);
                currentWorkout.LastUpdated = DateTime.UtcNow;

                // Find the new workout
                var newWorkout = await _mongoDbContext.Workouts
                    .FirstOrDefaultAsync(w => w.Id == newWorkoutId.Value);

                if (newWorkout == null)
                {
                    Console.WriteLine($"New Workout {newWorkoutId.Value} not found");
                    return;
                }

                // Initialize WeekPlans list if null
                if (newWorkout.WeekPlans == null)
                {
                    newWorkout.WeekPlans = new List<Databases.MongoDb.Collections.Workout.WeekPlan>();
                }

                // Create a NEW WeekPlan with updated WorkoutId (cannot modify FK in EF Core)
                var movedWeekPlan = new Databases.MongoDb.Collections.Workout.WeekPlan
                {
                    Id = weekPlanToUpdate.Id,
                    WorkoutId = newWorkoutId.Value,
                    DateOfWeek = weekPlanToUpdate.DateOfWeek,
                    Time = weekPlanToUpdate.Time,
                    CreatedDate = weekPlanToUpdate.CreatedDate,
                    LastUpdated = weekPlanToUpdate.LastUpdated,
                    WeekPlanSets = weekPlanSetsToKeep
                };

                // Add to new workout
                newWorkout.WeekPlans.Add(movedWeekPlan);
                newWorkout.LastUpdated = DateTime.UtcNow;

                _mongoDbContext.Workouts.UpdateRange(currentWorkout, newWorkout);
                await _mongoDbContext.SaveChangesAsync();

                Console.WriteLine($"Moved WeekPlan {message.weekPlanId} from Workout {currentWorkout.Id} to Workout {newWorkoutId.Value}");
            }
            else
            {
                // No move needed, just update in place
                currentWorkout.LastUpdated = DateTime.UtcNow;
                _mongoDbContext.Workouts.Update(currentWorkout);
                await _mongoDbContext.SaveChangesAsync();

                Console.WriteLine($"Patched WeekPlan {message.weekPlanId} in Workout {currentWorkout.Id}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"MongoDB error: {ex.Message}");
            throw;
        }
    }
}