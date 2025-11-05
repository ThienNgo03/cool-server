using Humanizer;
using Journal.Databases.MongoDb;
using Journal.Models.PaginationResults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Linq;
using System.Security.Claims;
using System.Threading.Channels;

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
    private readonly MongoDbContext _mongoDbContext;

    public Controller(IMessageBus messageBus,
                      ILogger<Controller> logger,
                      JournalDbContext context,
                      IHubContext<Hub> hubContext,
                      MongoDbContext mongoDbContext)
    {
        _messageBus = messageBus;
        _logger = logger;
        _context = context;
        _hubContext = hubContext;
        _mongoDbContext = mongoDbContext;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] Get.Parameters parameters)
    {
        var query = _context.Workouts.AsQueryable();
        var all = query;

        if (!string.IsNullOrEmpty(parameters.Ids))
        {
            var ids = parameters.Ids.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(id => Guid.TryParse(id.Trim(), out var guid) ? guid : (Guid?)null)
            .Where(guid => guid.HasValue)
            .Select(guid => guid.Value)
            .ToList();
            query = query.Where(x => ids.Contains(x.Id));
        }

        if (parameters.ExerciseId.HasValue)
            query = query.Where(x => x.ExerciseId == parameters.ExerciseId);

        if (parameters.UserId.HasValue)
            query = query.Where(x => x.UserId == parameters.UserId);

        if (parameters.CreatedDate.HasValue)
            query = query.Where(x => x.CreatedDate == parameters.CreatedDate);

        if (parameters.LastUpdated.HasValue)
            query = query.Where(x => x.LastUpdated == parameters.LastUpdated);

        if (!string.IsNullOrEmpty(parameters.SortBy))
        {
            var sortBy = typeof(Table)
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

        if (parameters.PageSize.HasValue && parameters.PageIndex.HasValue && parameters.PageSize > 0 && parameters.PageIndex.Value >= 0)
            query = query.Skip(parameters.PageSize.Value * parameters.PageIndex.Value).Take(parameters.PageSize.Value);

        var result = await query.AsNoTracking().ToListAsync();

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
            var includes = parameters.Include.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                 .Select(i => i.Trim().ToLower())
                                 .ToList();

            bool shouldFallbackToSql = false;

            try
            {
                var mongoWorkouts = await _mongoDbContext.Workouts.ToListAsync();

                if (mongoWorkouts == null || !mongoWorkouts.Any())
                {
                    shouldFallbackToSql = true;
                    throw new InvalidOperationException("MongoDB Workouts collection is empty");
                }

                var mongoWorkoutDict = mongoWorkouts.ToDictionary(w => w.Id);

                foreach (var inc in includes)
                {
                    var includeParts = inc.Split(".");
                    var mainInclude = includeParts[0];

                    if (mainInclude == "exercise")
                    {
                        foreach (var response in responses)
                        {
                            if (!mongoWorkoutDict.TryGetValue(response.Id, out var mongoWorkout))
                            {
                                shouldFallbackToSql = true;
                                break;
                            }

                            if (mongoWorkout.Exercise == null)
                            {
                                shouldFallbackToSql = true;
                                break;
                            }

                            response.Exercise = new Get.Exercise
                            {
                                Id = mongoWorkout.Exercise.Id,
                                Name = mongoWorkout.Exercise.Name,
                                Description = mongoWorkout.Exercise.Description,
                                Type = mongoWorkout.Exercise.Type,
                                CreatedDate = mongoWorkout.Exercise.CreatedDate,
                                LastUpdated = mongoWorkout.Exercise.LastUpdated,
                                Muscles = null
                            };

                            if (includeParts.Length > 1 && includeParts[1] == "muscles")
                            {
                                if (mongoWorkout.Exercise.Muscles == null || !mongoWorkout.Exercise.Muscles.Any())
                                {
                                    shouldFallbackToSql = true;
                                    break;
                                }

                                response.Exercise.Muscles = mongoWorkout.Exercise.Muscles.Select(m => new Get.Muscle
                                {
                                    Id = m.Id,
                                    Name = m.Name,
                                    CreatedDate = m.CreatedDate,
                                    LastUpdated = m.LastUpdated
                                }).ToList();
                            }
                        }

                        if (shouldFallbackToSql) break;
                    }
                    else if (mainInclude == "weekplans")
                    {
                        foreach (var response in responses)
                        {
                            if (!mongoWorkoutDict.TryGetValue(response.Id, out var mongoWorkout))
                            {
                                shouldFallbackToSql = true;
                                break;
                            }

                            if (mongoWorkout.WeekPlans == null || !mongoWorkout.WeekPlans.Any())
                            {
                                shouldFallbackToSql = true;
                                break;
                            }

                            response.WeekPlans = mongoWorkout.WeekPlans.Select(wp => new Get.WeekPlan
                            {
                                Id = wp.Id,
                                DateOfWeek = wp.DateOfWeek,
                                Time = wp.Time,
                                WorkoutId = wp.WorkoutId,
                                CreatedDate = wp.CreatedDate,
                                LastUpdated = wp.LastUpdated,
                                WeekPlanSets = null
                            }).ToList();

                            if (includeParts.Length > 1 && includeParts[1] == "weekplansets")
                            {
                                var mongoWeekPlanDict = mongoWorkout.WeekPlans.ToDictionary(wp => wp.Id);

                                foreach (var responseWeekPlan in response.WeekPlans)
                                {
                                    if (!mongoWeekPlanDict.TryGetValue(responseWeekPlan.Id, out var mongoWeekPlan))
                                    {
                                        shouldFallbackToSql = true;
                                        break;
                                    }

                                    if (mongoWeekPlan.WeekPlanSets == null || !mongoWeekPlan.WeekPlanSets.Any())
                                    {
                                        shouldFallbackToSql = true;
                                        break;
                                    }

                                    responseWeekPlan.WeekPlanSets = mongoWeekPlan.WeekPlanSets.Select(wps => new Get.WeekPlanSet
                                    {
                                        Id = wps.Id,
                                        Value = wps.Value,
                                        WeekPlanId = wps.WeekPlanId,
                                        InsertedBy = wps.InsertedBy,
                                        UpdatedBy = wps.UpdatedBy,
                                        LastUpdated = wps.LastUpdated,
                                        CreatedDate = wps.CreatedDate
                                    }).ToList();
                                }

                                if (shouldFallbackToSql) break;
                            }
                        }

                        if (shouldFallbackToSql) break;
                    }
                }

                if (shouldFallbackToSql)
                {
                    throw new InvalidOperationException("MongoDB data is incomplete, falling back to SQL");
                }

            }
            catch (Exception ex)
            {
                foreach (var response in responses)
                {
                    response.Exercise = null;
                    response.WeekPlans = null;
                }

                var exerciseIds = result.Select(w => w.ExerciseId).Distinct().ToList();

                foreach (var inc in includes)
                {
                    Dictionary<Guid, Get.Exercise> exercisesByExerciseId = new();
                    if (inc.Split(".")[0] == "exercise")
                    {
                        var exercises = await _context.Exercises
                            .Where(e => exerciseIds.Contains(e.Id))
                            .ToListAsync();

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
        }

        var paginationResults = new Builder<Get.Response>()
            .WithAll(await all.CountAsync())
            .WithIndex(parameters.PageIndex)
            .WithSize(parameters.PageSize)
            .WithTotal(responses.Count)
            .WithItems(responses)
            .Build();

        return Ok(paginationResults);
    }

    [HttpPost("sync-data-to-mongodb")]
    public async Task<IActionResult> SyncData()
    {
        try
        {
            var workouts = await _context.Workouts.AsNoTracking().ToListAsync();
            var workoutIds = workouts.Select(x => x.Id).ToList();

            if (!workoutIds.Any())
                return Ok("No workouts to sync.");

            var weekPlans = await _context.WeekPlans
                .Where(wp => workoutIds.Contains(wp.WorkoutId))
                .ToListAsync();

            var weekPlanIds = weekPlans.Select(wp => wp.Id).ToList();

            var weekPlanSets = await _context.WeekPlanSets
                .Where(wps => weekPlanIds.Contains(wps.WeekPlanId))
                .ToListAsync();

            var exerciseIds = workouts.Select(w => w.ExerciseId).Distinct().ToList();
            var exercises = await _context.Exercises
                .Where(e => exerciseIds.Contains(e.Id))
                .ToListAsync();

            var exerciseMuscles = await _context.ExerciseMuscles
                .Where(em => exerciseIds.Contains(em.ExerciseId))
                .ToListAsync();

            var muscleIds = exerciseMuscles.Select(em => em.MuscleId).Distinct().ToList();
            var muscles = await _context.Muscles
                .Where(m => muscleIds.Contains(m.Id))
                .ToDictionaryAsync(m => m.Id);

            var musclesByExerciseId = exerciseMuscles
                .GroupBy(em => em.ExerciseId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Where(em => muscles.ContainsKey(em.MuscleId))
                          .Select(em => new Journal.Databases.MongoDb.Collections.Workout.Muscle
                          {
                              Id = muscles[em.MuscleId].Id,
                              Name = muscles[em.MuscleId].Name,
                              CreatedDate = muscles[em.MuscleId].CreatedDate,
                              LastUpdated = muscles[em.MuscleId].LastUpdated
                          }).ToList()
                );

            var weekPlanSetsByWeekPlanId = weekPlanSets
                .GroupBy(wps => wps.WeekPlanId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(wps => new Journal.Databases.MongoDb.Collections.Workout.WeekPlanSet
                    {
                        Id = wps.Id,
                        Value = wps.Value,
                        WeekPlanId = wps.WeekPlanId,
                        InsertedBy = wps.InsertedBy,
                        UpdatedBy = wps.UpdatedBy,
                        LastUpdated = wps.LastUpdated,
                        CreatedDate = wps.CreatedDate
                    }).ToList()
                );

            var weekPlansByWorkoutId = weekPlans
                .GroupBy(wp => wp.WorkoutId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(wp => new Journal.Databases.MongoDb.Collections.Workout.WeekPlan
                    {
                        Id = wp.Id,
                        DateOfWeek = wp.DateOfWeek,
                        Time = wp.Time,
                        WorkoutId = wp.WorkoutId,
                        CreatedDate = wp.CreatedDate,
                        LastUpdated = wp.LastUpdated,
                        WeekPlanSets = weekPlanSetsByWeekPlanId.GetValueOrDefault(wp.Id, new List<Journal.Databases.MongoDb.Collections.Workout.WeekPlanSet>())
                    }).ToList()
                );

            var workoutCollections = new List<Journal.Databases.MongoDb.Collections.Workout.Collection>();

            foreach (var workout in workouts)
            {
                var exercise = exercises.FirstOrDefault(e => e.Id == workout.ExerciseId);

                var workoutCollection = new Journal.Databases.MongoDb.Collections.Workout.Collection
                {
                    Id = workout.Id,
                    ExerciseId = workout.ExerciseId,
                    UserId = workout.UserId,
                    CreatedDate = workout.CreatedDate,
                    LastUpdated = workout.LastUpdated,
                    Exercise = exercise != null ? new Journal.Databases.MongoDb.Collections.Workout.Exercise
                    {
                        Id = exercise.Id,
                        Name = exercise.Name,
                        Description = exercise.Description,
                        Type = exercise.Type,
                        CreatedDate = exercise.CreatedDate,
                        LastUpdated = exercise.LastUpdated,
                        Muscles = musclesByExerciseId.GetValueOrDefault(exercise.Id, new List<Journal.Databases.MongoDb.Collections.Workout.Muscle>())
                    } : null,
                    WeekPlans = weekPlansByWorkoutId.GetValueOrDefault(workout.Id, new List<Journal.Databases.MongoDb.Collections.Workout.WeekPlan>())
                };

                workoutCollections.Add(workoutCollection);
            }

            _mongoDbContext.Workouts.RemoveRange(_mongoDbContext.Workouts);
            _mongoDbContext.Workouts.AddRange(workoutCollections);

            var savedCount = await _mongoDbContext.SaveChangesAsync();

            return Ok($"Successfully synced {savedCount} workouts to MongoDB.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error syncing workouts: {ex.Message}");
        }
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
        var workout = new Table
        {
            Id = Guid.NewGuid(),
            ExerciseId = payload.ExerciseId,
            UserId = payload.UserId,
            CreatedDate = DateTime.UtcNow,
            LastUpdated = DateTime.UtcNow
        };

        _context.Workouts.Add(workout);
        await _context.SaveChangesAsync();
        await _messageBus.PublishAsync(new Post.Messager.Message(workout, payload.WeekPlans));
        await _hubContext.Clients.All.SendAsync("workout-created", workout.Id);
        return CreatedAtAction(nameof(Get), workout.Id);
    }

    [HttpPatch]
    public async Task<IActionResult> Patch([FromQuery] Guid id,
                                       [FromBody] JsonPatchDocument<Table> patchDoc,
                                       CancellationToken cancellationToken = default!)
    {
        if (User.Identity is null)
            return Unauthorized();

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId is null)
            return Unauthorized("User Id not found");

        var changes = new List<(string Path, object? Value)>();
        foreach (var op in patchDoc.Operations)
        {
            if (op.OperationType != OperationType.Replace && op.OperationType != OperationType.Test)
                return BadRequest("Only Replace and Test operations are allowed in this patch request.");
            changes.Add((op.path, op.value));
        }
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
        await _messageBus.PublishAsync(new Patch.Messager.Message(entity, changes));
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
        await _messageBus.PublishAsync(new Update.Messager.Message(workout));
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
            var workouts = await _context.Workouts.ToListAsync();
            _context.Workouts.RemoveRange(workouts);
            await _context.SaveChangesAsync();
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
