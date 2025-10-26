using Journal.Models.PaginationResults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Journal.Workouts;

[ApiController]
[Authorize]
[Route("api/workouts")]
public class Controller : ControllerBase
{
    private readonly IMessageBus _messageBus;
    private readonly ILogger<Controller> _logger;
    private readonly JournalDbContext _context;
    private readonly IHubContext<Hub> _hubContext;

    public Controller(IMessageBus messageBus, ILogger<Controller> logger, JournalDbContext context, IHubContext<Hub> hubContext)
    {
        _messageBus = messageBus;
        _logger = logger;
        _context = context;
        _hubContext = hubContext;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] Get.Parameters parameters)
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
        List<Get.Response> responses = result.Select(item => new Get.Response
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
                Dictionary<Guid, Get.Exercise> exercisesByExerciseId = new();
                if (inc.Split(".")[0] == "exercise")
                {
                    var exerciseIds = result.Select(w => w.ExerciseId).Distinct().ToList();
                    var exercises = await _context.Exercises
                        .Where(e => exerciseIds.Contains(e.Id))
                        .ToListAsync();

                    // Fetch exercise muscles if requested
                    Dictionary<Guid, List<Get.Muscle>> musclesByExerciseId = new();
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
                                musclesByExerciseId[relation.ExerciseId] = new List<Get.Muscle>();

                            if (muscles.TryGetValue(relation.MuscleId, out var muscle))
                            {
                                musclesByExerciseId[relation.ExerciseId].Add(new Get.Muscle
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
                        exercisesByExerciseId[exercise.Id] = new Get.Exercise
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

                Dictionary<Guid, List<Get.WeekPlan>> workoutWeekPlans = new();
                if (inc.Split(".")[0] == "weekplans")
                {
                    var workoutIds = result.Select(w => w.Id).ToList();
                    var weekPlans = await _context.WeekPlans
                        .Where(wp => workoutIds.Contains(wp.WorkoutId))
                        .ToListAsync();

                    var weekPlanIds = weekPlans.Select(wp => wp.Id).ToList();

                    // Only fetch WeekPlanSets if requested
                    Dictionary<Guid, List<Get.WeekPlanSet>> weekPlanSetsByWeekPlanId = new();
                    if (inc.Split(".").Length > 1 && inc.Split(".")[1] == "weekplansets")
                    {
                        var weekPlanSets = await _context.WeekPlanSets
                            .Where(wps => weekPlanIds.Contains(wps.WeekPlanId))
                            .ToListAsync();

                        foreach (var set in weekPlanSets)
                        {
                            if (!weekPlanSetsByWeekPlanId.ContainsKey(set.WeekPlanId))
                                weekPlanSetsByWeekPlanId[set.WeekPlanId] = new List<Get.WeekPlanSet>();

                            weekPlanSetsByWeekPlanId[set.WeekPlanId].Add(new Get.WeekPlanSet
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
                            workoutWeekPlans[weekPlan.WorkoutId] = new List<Get.WeekPlan>();

                        var weekPlanModel = new Get.WeekPlan
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

        var paginationResults = new Builder<Get.Response>()
            .WithIndex(parameters.PageIndex)
            .WithSize(parameters.PageSize)
            .WithTotal(responses.Count)
            .WithItems(responses)
            .Build();

        return Ok(paginationResults);
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] Post.Payload payload)
    {
        if (User.Identity is null)
            return Unauthorized();

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId is null)
            return Unauthorized("User Id not found");

        var existingExercise = await _context.Exercises.FindAsync(payload.ExerciseId);
        if (existingExercise == null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Exercise not found",
                Detail = $"Exercise with ID {payload.ExerciseId} does not exist.",
                Status = StatusCodes.Status404NotFound,
                Instance = HttpContext.Request.Path
            });
        }
        var existingUser = await _context.Users.FindAsync(payload.UserId);
        if (existingUser == null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "User not found",
                Detail = $"User with ID {payload.UserId} does not exist.",
                Status = StatusCodes.Status404NotFound,
                Instance = HttpContext.Request.Path
            });
        }
        var workout = new Databases.App.Tables.Workout.Table
        {
            Id = Guid.NewGuid(),
            ExerciseId = payload.ExerciseId,
            UserId = payload.UserId,
            CreatedDate = DateTime.UtcNow,
            LastUpdated = DateTime.UtcNow
        };

        _context.Workouts.Add(workout);
        await _context.SaveChangesAsync();
        await _messageBus.PublishAsync(new Post.Messager.Message(workout.Id, payload.WeekPlans));
        await _hubContext.Clients.All.SendAsync("workout-created", workout.Id);
        return CreatedAtAction(nameof(Get), workout.Id);
    }

    [HttpPatch]
    public async Task<IActionResult> Patch([FromQuery] Guid id,
                                       [FromBody] JsonPatchDocument<Databases.App.Tables.Workout.Table> patchDoc,
                                       CancellationToken cancellationToken = default!)
    {
        if (User.Identity is null)
            return Unauthorized();

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId is null)
            return Unauthorized("User Id not found");

        foreach (var op in patchDoc.Operations)
            if (op.OperationType != OperationType.Replace && op.OperationType != OperationType.Test)
                return BadRequest("Only Replace and Test operations are allowed in this patch request.");

        if (patchDoc is null)
            return BadRequest("Patch document cannot be null.");

        var entity = await _context.Workouts.FindAsync(id, cancellationToken);
        if (entity == null)
            return NotFound(new ProblemDetails
            {
                Title = "Workout not found",
                Detail = $"Workout with ID {id} does not exist.",
                Status = StatusCodes.Status404NotFound,
                Instance = HttpContext.Request.Path
            });

        patchDoc.ApplyTo(entity);

        entity.LastUpdated = DateTime.UtcNow;

        _context.Workouts.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
        await _hubContext.Clients.All.SendAsync("workout-updated", entity.Id);

        return NoContent();
    }

    [HttpPut]
    public async Task<IActionResult> Put([FromBody] Update.Payload payload)
    {
        if (User.Identity is null)
            return Unauthorized();

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId is null)
            return Unauthorized("User Id not found");

        var workout = await _context.Workouts.FindAsync(payload.Id);
        if (workout == null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Workout not found",
                Detail = $"Workout with ID {payload.Id} does not exist.",
                Status = StatusCodes.Status404NotFound,
                Instance = HttpContext.Request.Path
            });
        }
        var existingExercise = await _context.Exercises.FindAsync(payload.ExerciseId);
        if (existingExercise == null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Exercise not found",
                Detail = $"Exercise with ID {payload.ExerciseId} does not exist.",
                Status = StatusCodes.Status404NotFound,
                Instance = HttpContext.Request.Path
            });
        }
        var existingUser = await _context.Users.FindAsync(payload.UserId);
        if (existingUser == null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "User not found",
                Detail = $"User with ID {payload.UserId} does not exist.",
                Status = StatusCodes.Status404NotFound,
                Instance = HttpContext.Request.Path
            });
        }

        workout.ExerciseId = payload.ExerciseId;
        workout.UserId = payload.UserId;
        workout.LastUpdated = DateTime.UtcNow;
        _context.Workouts.Update(workout);
        await _context.SaveChangesAsync();
        await _messageBus.PublishAsync(new Update.Messager.Message(payload.Id));
        await _hubContext.Clients.All.SendAsync("workout-updated", payload.Id);
        return NoContent();
    }

    [HttpDelete]
    public async Task<IActionResult> Delete([FromQuery] Delete.Parameters parameters)
    {
        if (User.Identity is null)
            return Unauthorized();

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId is null)
            return Unauthorized("User Id not found");
        if (parameters.IsDeleteAll)
        {
            await _context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE Workouts");
            return NoContent();
        }

        var workout = await _context.Workouts.FindAsync(parameters.Id);
        if (workout == null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Workout not found",
                Detail = $"Workout with ID {parameters.Id} does not exist.",
                Status = StatusCodes.Status404NotFound,
                Instance = HttpContext.Request.Path
            });
        }

        _context.Workouts.Remove(workout);
        await _context.SaveChangesAsync();
        await _messageBus.PublishAsync(new Delete.Messager.Message(parameters.Id, 
                                                                   parameters.IsWeekPlanDelete, 
                                                                   parameters.IsWeekPlanSetDelete));
        await _hubContext.Clients.All.SendAsync("workout-deleted", parameters.Id);
        return NoContent();
    }
}
