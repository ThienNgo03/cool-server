namespace Journal.WeekPlanSets.Patch.Messager;

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

        // Get the updated weekPlanSet from database
        var updatedWeekPlanSet = await _context.WeekPlanSets.FirstOrDefaultAsync(wps => wps.Id == message.weekPlanSetId);
        if (updatedWeekPlanSet == null)
        {
            Console.WriteLine($"WeekPlanSet {message.weekPlanSetId} not found.");
            return;
        }

        // ===== SYNC MONGODB =====
        try
        {
            // Find the current workout that has this WeekPlanSet
            var currentWorkout = await _mongoDbContext.Workouts
                .FirstOrDefaultAsync(w => w.WeekPlans != null &&
                    w.WeekPlans.Any(wp => wp.WeekPlanSets != null &&
                        wp.WeekPlanSets.Any(wps => wps.Id == message.weekPlanSetId)));

            if (currentWorkout == null)
            {
                Console.WriteLine($"Workout with WeekPlanSet {message.weekPlanSetId} not found");
                return;
            }

            var currentWeekPlan = currentWorkout.WeekPlans?
                .FirstOrDefault(wp => wp.WeekPlanSets != null &&
                    wp.WeekPlanSets.Any(wps => wps.Id == message.weekPlanSetId));

            if (currentWeekPlan == null)
            {
                Console.WriteLine($"WeekPlan with WeekPlanSet {message.weekPlanSetId} not found");
                return;
            }

            var weekPlanSetToUpdate = currentWeekPlan.WeekPlanSets?.FirstOrDefault(wps => wps.Id == message.weekPlanSetId);
            if (weekPlanSetToUpdate == null)
            {
                Console.WriteLine($"WeekPlanSet {message.weekPlanSetId} not found in WeekPlan");
                return;
            }

            // Check if WeekPlanId is being changed
            bool weekPlanIdChanged = false;
            Guid? newWeekPlanId = null;

            foreach (var (path, value) in message.changes)
            {
                var fieldName = path.TrimStart('/').ToLowerInvariant();

                if (fieldName == "weekplanid")
                {
                    if (value is string guidString && Guid.TryParse(guidString, out Guid guidValue))
                    {
                        newWeekPlanId = guidValue;
                        weekPlanIdChanged = currentWeekPlan.Id != guidValue;
                    }
                    continue; // Don't apply WeekPlanId change yet
                }

                // Apply other changes
                switch (fieldName)
                {
                    case "value":
                        if (value is long longValue)
                        {
                            weekPlanSetToUpdate.Value = (int)longValue;
                        }
                         break;
                        // Add other patchable fields as needed
                }
            }

            // Always update lastUpdated
            weekPlanSetToUpdate.LastUpdated = updatedWeekPlanSet.LastUpdated;

            // If WeekPlanId changed, move the WeekPlanSet to the new WeekPlan
            if (weekPlanIdChanged && newWeekPlanId.HasValue)
            {
                // Remove from current WeekPlan
                currentWeekPlan.WeekPlanSets?.RemoveAll(wps => wps.Id == message.weekPlanSetId);
                currentWeekPlan.LastUpdated = DateTime.UtcNow;

                // Find the new WeekPlan (might be in a different workout)
                var newWorkout = await _mongoDbContext.Workouts
                    .FirstOrDefaultAsync(w => w.WeekPlans != null &&
                        w.WeekPlans.Any(wp => wp.Id == newWeekPlanId.Value));

                if (newWorkout == null)
                {
                    Console.WriteLine($"Workout with new WeekPlan {newWeekPlanId.Value} not found");
                    return;
                }

                var newWeekPlan = newWorkout.WeekPlans?.FirstOrDefault(wp => wp.Id == newWeekPlanId.Value);
                if (newWeekPlan == null)
                {
                    Console.WriteLine($"New WeekPlan {newWeekPlanId.Value} not found in workout");
                    return;
                }

                // Initialize WeekPlanSets list if null
                if (newWeekPlan.WeekPlanSets == null)
                {
                    newWeekPlan.WeekPlanSets = new List<Databases.MongoDb.Collections.Workout.WeekPlanSet>();
                }

                // Create a NEW WeekPlanSet with updated WeekPlanId (cannot modify FK in EF Core)
                var movedWeekPlanSet = new Databases.MongoDb.Collections.Workout.WeekPlanSet
                {
                    Id = weekPlanSetToUpdate.Id,
                    WeekPlanId = newWeekPlanId.Value,
                    Value = weekPlanSetToUpdate.Value,
                    CreatedDate = weekPlanSetToUpdate.CreatedDate,
                    LastUpdated = weekPlanSetToUpdate.LastUpdated
                };

                // Add to new WeekPlan
                newWeekPlan.WeekPlanSets.Add(movedWeekPlanSet);
                newWeekPlan.LastUpdated = DateTime.UtcNow;
                newWorkout.LastUpdated = DateTime.UtcNow;

                // Update both workouts (if they're different)
                if (currentWorkout.Id != newWorkout.Id)
                {
                    _mongoDbContext.Workouts.UpdateRange(currentWorkout, newWorkout);
                }
                else
                {
                    _mongoDbContext.Workouts.Update(newWorkout);
                }

                await _mongoDbContext.SaveChangesAsync();

                Console.WriteLine($"Moved WeekPlanSet {message.weekPlanSetId} from WeekPlan {currentWeekPlan.Id} to WeekPlan {newWeekPlanId.Value}");
            }
            else
            {
                // No move needed, just update in place
                currentWeekPlan.LastUpdated = DateTime.UtcNow;
                currentWorkout.LastUpdated = DateTime.UtcNow;
                _mongoDbContext.Workouts.Update(currentWorkout);
                await _mongoDbContext.SaveChangesAsync();

                Console.WriteLine($"Patched WeekPlanSet {message.weekPlanSetId} in WeekPlan {currentWeekPlan.Id}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"MongoDB error: {ex.Message}");
            throw;
        }
    }
}