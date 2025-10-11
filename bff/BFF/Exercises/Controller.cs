using BFF.Databases.App;
using BFF.Exercises.GetExercises;
using BFF.Models.PaginationResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BFF.Exercises;

[Route("api/exercises")]
[ApiController]
public class Controller : ControllerBase
{
    private readonly JournalDbContext _context;
    public Controller(JournalDbContext context)
    {
        _context = context;
    }

    [HttpGet("get-exercises")]
    public async Task<IActionResult> GetExercises([FromQuery] Parameters parameters)
    {
        var query = _context.Exercises.AsQueryable();

        if (!string.IsNullOrEmpty(parameters.Ids))
        {
            var ids = parameters.Ids.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(id => Guid.TryParse(id.Trim(), out var guid) ? guid : (Guid?)null)
                        .Where(guid => guid.HasValue)
                        .Select(guid => guid.Value)
                        .ToList();
            query = query.Where(x => ids.Contains(x.Id));
        }

        if (!string.IsNullOrEmpty(parameters.Name))
            query = query.Where(x => x.Name.Contains(parameters.Name));

        if (!string.IsNullOrEmpty(parameters.Description))
            query = query.Where(x => x.Description.Contains(parameters.Description));

        if (!string.IsNullOrEmpty(parameters.Type))
            query = query.Where(x => x.Type.Contains(parameters.Type));

        if (parameters.CreatedDate.HasValue)
            query = query.Where(x => x.CreatedDate == parameters.CreatedDate);

        if (parameters.LastUpdated.HasValue)
            query = query.Where(x => x.LastUpdated == parameters.LastUpdated);

        if (parameters.PageSize.HasValue && parameters.PageIndex.HasValue && parameters.PageSize > 0 && parameters.PageIndex >= 0)
            query = query.Skip(parameters.PageIndex.Value * parameters.PageSize.Value).Take(parameters.PageSize.Value);

        if (!string.IsNullOrEmpty(parameters.SortBy))
        {
            var sortBy = typeof(Databases.App.Tables.Exercise.Table)
                .GetProperties()
                .FirstOrDefault(p => p.Name.Equals(parameters.SortBy, StringComparison.OrdinalIgnoreCase))
                ?.Name;
            if (sortBy != null)
            {
                query = parameters.SortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(x => EF.Property<object>(x, sortBy))
                    : query.OrderBy(x => EF.Property<object>(x, sortBy));
            }
        }

        var result = await query.AsNoTracking().ToListAsync();
        var exerciseIds = result.Select(x => x.Id).ToList();

        var responses = result.Select(exercise => new GetExercises.Response
        {
            Id = exercise.Id,
            Name = exercise.Name,
            Description = exercise.Description,
            Type = exercise.Type,
            CreatedDate = exercise.CreatedDate,
            LastUpdated = exercise.LastUpdated
        }).ToList();

        var paginationResults = new Builder<Response>()
            .WithIndex(parameters.PageIndex)
            .WithSize(parameters.PageSize)
            .WithTotal(responses.Count)
            .WithItems(responses)
            .Build();

        if (string.IsNullOrEmpty(parameters.Include))
        {
            return Ok(paginationResults);
        }

        var includes = parameters.Include.Split(',', StringSplitOptions.RemoveEmptyEntries)
                             .Select(i => i.Trim().ToLower())
                             .ToList();

        if (!includes.Any(inc => inc.Split(".")[0] == "muscles") || !exerciseIds.Any())
        {
            return Ok(paginationResults);
        }

        var exerciseMusclesTask = _context.ExerciseMuscles
            .Where(x => exerciseIds.Contains(x.ExerciseId))
            .ToListAsync();

        var exerciseMuscles = await exerciseMusclesTask;
        var muscleIds = exerciseMuscles.Select(x => x.MuscleId).Distinct().ToList();

        if (!muscleIds.Any())
        {
            return Ok(paginationResults);
        }

        var muscles = await _context.Muscles
            .Where(x => muscleIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id);

        var exerciseMuscleGroups = exerciseMuscles
            .GroupBy(x => x.ExerciseId)
            .ToDictionary(g => g.Key, g => g.Select(em => em.MuscleId));

        foreach (var response in responses)
        {
            if (!exerciseMuscleGroups.TryGetValue(response.Id, out var responseMuscleIds))
            {
                continue;
            }

            response.Muscles = responseMuscleIds
                .Where(muscleId => muscles.ContainsKey(muscleId))
                .Select(muscleId => new GetExercises.Muscle
                {
                    Id = muscles[muscleId].Id,
                    Name = muscles[muscleId].Name,
                    CreatedDate = muscles[muscleId].CreatedDate,
                    LastUpdated = muscles[muscleId].LastUpdated
                })
                .ToList();
        }

        if (string.IsNullOrEmpty(parameters.MusclesSortBy))
            return Ok(paginationResults);

        var normalizeProp = typeof(Databases.App.Tables.Muscle.Table)
            .GetProperties()
            .FirstOrDefault(p => p.Name.Equals(parameters.MusclesSortBy, StringComparison.OrdinalIgnoreCase))
            ?.Name;

        if (normalizeProp == null)
            return Ok(paginationResults);

        var prop = typeof(Databases.App.Tables.Muscle.Table).GetProperty(normalizeProp);
        if (prop == null)
            return Ok(paginationResults);

        var isDescending = parameters.MusclesSortOrder?.ToLower() == "desc";
        foreach (var response in responses.Where(r => r.Muscles?.Any() == true))
        {
            if (response.Muscles == null || !response.Muscles.Any())
                continue;
            response.Muscles = isDescending
                ? response.Muscles.OrderByDescending(m => prop.GetValue(m)).ToList()
                : response.Muscles.OrderBy(m => prop.GetValue(m)).ToList();
        }

        return Ok(paginationResults);
    }

    [HttpGet("get-workouts")]
    public async Task<IActionResult> GetWorkouts([FromQuery] GetWorkouts.Parameters parameters)
    {
        var query = _context.Workouts.AsQueryable();

        if (parameters.Id.HasValue)
            query = query.Where(x => x.Id == parameters.Id);
        if (parameters.ExerciseId.HasValue)
            query = query.Where(x => x.ExerciseId == parameters.ExerciseId);
        if (parameters.UserId.HasValue)
            query = query.Where(x => x.UserId == parameters.UserId);
        if (parameters.CreatedDate.HasValue)
            query = query.Where(x => x.CreatedDate == parameters.CreatedDate);
        if (parameters.LastUpdated.HasValue)
            query = query.Where(x => x.LastUpdated == parameters.LastUpdated);

        if (parameters.PageSize.HasValue && parameters.PageIndex.HasValue && parameters.PageSize > 0 && parameters.PageIndex.Value >= 0)
            query = query.Skip(parameters.PageSize.Value * parameters.PageIndex.Value).Take(parameters.PageSize.Value);

        var result = await query.AsNoTracking().ToListAsync();

        // Initialize responses list outside the if condition
        List<GetWorkouts.Response> responses = result.Select(item => new GetWorkouts.Response
        {
            Id = item.Id,
            ExerciseId = item.ExerciseId,
            UserId = item.UserId,
            CreatedDate = item.CreatedDate,
            LastUpdated = item.LastUpdated,
            Exercise = null,
            WeekPlans = null
        }).ToList();

        if (!string.IsNullOrEmpty(parameters.Include))
        {
            // Split the include parameter into a list
            var includes = parameters.Include.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                 .Select(i => i.Trim().ToLower())
                                 .ToList();

            // Dynamically apply Include for valid navigation properties
            foreach (var inc in includes)
            {
                Dictionary<Guid, GetWorkouts.Exercise> exercisesByExerciseId = new();
                if (inc.Split(".")[0] == "exercise")
                {
                    var exerciseIds = result.Select(w => w.ExerciseId).Distinct().ToList();
                    var exercises = await _context.Exercises
                        .Where(e => exerciseIds.Contains(e.Id))
                        .ToListAsync();

                    // Fetch exercise muscles if requested
                    Dictionary<Guid, List<GetWorkouts.Muscle>> musclesByExerciseId = new();
                    if (inc.Split(".").Length > 1 && inc.Split(".")[1] == "muscles")
                    {
                        var exerciseMuscleRelations = await _context.ExerciseMuscles
                            .Where(em => exerciseIds.Contains(em.ExerciseId))
                            .ToListAsync();

                        var muscleIds = exerciseMuscleRelations.Select(em => em.MuscleId).Distinct().ToList();
                        var muscles = await _context.Muscles
                            .Where(m => muscleIds.Contains(m.Id))
                            .ToDictionaryAsync(m => m.Id);

                        // Group muscles by exercise ID
                        foreach (var relation in exerciseMuscleRelations)
                        {
                            if (!musclesByExerciseId.ContainsKey(relation.ExerciseId))
                                musclesByExerciseId[relation.ExerciseId] = new List<GetWorkouts.Muscle>();

                            if (muscles.TryGetValue(relation.MuscleId, out var muscle))
                            {
                                musclesByExerciseId[relation.ExerciseId].Add(new GetWorkouts.Muscle
                                {
                                    Id = muscle.Id,
                                    Name = muscle.Name,
                                    CreatedDate = muscle.CreatedDate,
                                    LastUpdated = muscle.LastUpdated
                                });
                            }
                        }
                    }

                    // Create exercise models with their muscles
                    foreach (var exercise in exercises)
                    {
                        exercisesByExerciseId[exercise.Id] = new GetWorkouts.Exercise
                        {
                            Id = exercise.Id,
                            Name = exercise.Name,
                            Description = exercise.Description,
                            Type = exercise.Type,
                            CreatedDate = exercise.CreatedDate,
                            LastUpdated = exercise.LastUpdated,
                            Muscles = musclesByExerciseId.ContainsKey(exercise.Id)
                                     ? musclesByExerciseId[exercise.Id]
                                     : null
                        };
                    }

                    // Update responses with exercises
                    foreach (var response in responses)
                    {
                        if (exercisesByExerciseId.ContainsKey(response.ExerciseId))
                        {
                            response.Exercise = exercisesByExerciseId[response.ExerciseId];
                        }
                    }
                }

                Dictionary<Guid, List<GetWorkouts.WeekPlan>> workoutWeekPlans = new();
                if (inc.Split(".")[0] == "weekplans")
                {
                    var workoutIds = result.Select(w => w.Id).ToList();
                    var weekPlans = await _context.WeekPlans
                        .Where(wp => workoutIds.Contains(wp.WorkoutId))
                        .ToListAsync();

                    var weekPlanIds = weekPlans.Select(wp => wp.Id).ToList();

                    // Only fetch WeekPlanSets if requested
                    Dictionary<Guid, List<GetWorkouts.WeekPlanSet>> weekPlanSetsByWeekPlanId = new();
                    if (inc.Split(".").Length > 1 && inc.Split(".")[1] == "weekplansets")
                    {
                        var weekPlanSets = await _context.WeekPlanSets
                            .Where(wps => weekPlanIds.Contains(wps.WeekPlanId))
                            .ToListAsync();

                        foreach (var set in weekPlanSets)
                        {
                            if (!weekPlanSetsByWeekPlanId.ContainsKey(set.WeekPlanId))
                                weekPlanSetsByWeekPlanId[set.WeekPlanId] = new List<GetWorkouts.WeekPlanSet>();

                            weekPlanSetsByWeekPlanId[set.WeekPlanId].Add(new GetWorkouts.WeekPlanSet
                            {
                                Id = set.Id,
                                Value = set.Value,
                                WeekPlanId = set.WeekPlanId,
                                InsertedBy = set.InsertedBy,
                                UpdatedBy = set.UpdatedBy,
                                LastUpdated = set.LastUpdated,
                                CreatedDate = set.CreatedDate
                            });
                        }
                    }

                    // Group week plans by workout ID
                    foreach (var weekPlan in weekPlans)
                    {
                        if (!workoutWeekPlans.ContainsKey(weekPlan.WorkoutId))
                            workoutWeekPlans[weekPlan.WorkoutId] = new List<GetWorkouts.WeekPlan>();

                        var weekPlanModel = new GetWorkouts.WeekPlan
                        {
                            Id = weekPlan.Id,
                            DateOfWeek = weekPlan.DateOfWeek,
                            Time = weekPlan.Time,
                            WorkoutId = weekPlan.WorkoutId,
                            CreatedDate = weekPlan.CreatedDate,
                            LastUpdated = weekPlan.LastUpdated,
                            WeekPlanSets = weekPlanSetsByWeekPlanId.ContainsKey(weekPlan.Id)
                                          ? weekPlanSetsByWeekPlanId[weekPlan.Id]
                                          : null
                        };
                        workoutWeekPlans[weekPlan.WorkoutId].Add(weekPlanModel);
                    }

                    // Update responses with week plans
                    foreach (var response in responses)
                    {
                        if (workoutWeekPlans.ContainsKey(response.Id))
                        {
                            response.WeekPlans = workoutWeekPlans[response.Id];
                        }
                    }
                }
            }
        }

        var paginationResults = new Builder<GetWorkouts.Response>()
            .WithIndex(parameters.PageIndex)
            .WithSize(parameters.PageSize)
            .WithTotal(responses.Count)
            .WithItems(responses)
            .Build();

        return Ok(paginationResults);
    }
}
