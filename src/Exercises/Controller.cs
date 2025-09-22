using Journal.Models.PaginationResults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Journal.Exercises;

[ApiController]
[Authorize]
[Route("api/exercises")]
public class Controller : ControllerBase
{
    private readonly IMessageBus _messageBus;
    private readonly JournalDbContext _context;
    private readonly ILogger<Controller> _logger;
    private readonly IHubContext<Hub> _hubContext;

    public Controller(IMessageBus messageBus, JournalDbContext context, ILogger<Controller> logger, IHubContext<Hub> hubContext)
    {
        _context = context;
        _logger = logger;
        _messageBus = messageBus;
        _hubContext = hubContext;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] Get.Parameters parameters)
    {
        var query = _context.Exercises.AsQueryable();

        if (parameters.Id.HasValue)
            query = query.Where(x => x.Id == parameters.Id);

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
            var sortBy = typeof(Databases.Journal.Tables.Exercise.Table)
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

        // Create response model to include muscles
        var responses = result.Select(exercise => new Get.Response
        {
            Id = exercise.Id,
            Name = exercise.Name,
            Description = exercise.Description,
            Type = exercise.Type,
            CreatedDate = exercise.CreatedDate,
            LastUpdated = exercise.LastUpdated
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
                if (inc.Split(".")[0] == "muscles" && exerciseIds.Any())
                {
                    // Get all related exercise muscles in one query
                    var exerciseMuscles = await _context.ExerciseMuscles
                        .Where(x => exerciseIds.Contains(x.ExerciseId))
                        .ToListAsync();

                    var muscleIds = exerciseMuscles.Select(x => x.MuscleId).Distinct().ToList();

                    // Get all related muscles in one query
                    var muscles = await _context.Muscles
                        .Where(x => muscleIds.Contains(x.Id))
                        .ToDictionaryAsync(x => x.Id);

                    // Group exercise muscles by exercise ID
                    var exerciseMuscleGroups = exerciseMuscles
                        .GroupBy(x => x.ExerciseId)
                        .ToDictionary(g => g.Key, g => g.Select(em => em.MuscleId));

                    // Attach muscles to each response
                    foreach (var response in responses)
                    {
                        if (exerciseMuscleGroups.TryGetValue(response.Id, out var responseMuscleIds))
                        {
                            response.Muscles = responseMuscleIds
                                .Where(muscleId => muscles.ContainsKey(muscleId))
                                .Select(muscleId => new Get.Muscle
                                {
                                    Id = muscles[muscleId].Id,
                                    Name = muscles[muscleId].Name,
                                    CreatedDate = muscles[muscleId].CreatedDate,
                                    LastUpdated = muscles[muscleId].LastUpdated
                                })
                                .ToList();
                        }
                    }
                }
            }
        }

        if (!string.IsNullOrEmpty(parameters.MusclesSortBy))
        {
            var normalizeProp = typeof(Databases.Journal.Tables.Muscle.Table)
                .GetProperties()
                .FirstOrDefault(p => p.Name.Equals(parameters.MusclesSortBy, StringComparison.OrdinalIgnoreCase))
                ?.Name;
            if (normalizeProp != null)
            {
                var prop = typeof(Databases.Journal.Tables.Muscle.Table).GetProperty(normalizeProp);
                if (prop != null)
                {
                    foreach (var response in responses)
                    {
                        if (response.Muscles != null)
                        {
                            response.Muscles = parameters.MusclesSortOrder?.ToLower() == "desc"
                                ? response.Muscles.OrderByDescending(m => prop.GetValue(m)).ToList()
                                : response.Muscles.OrderBy(m => prop.GetValue(m)).ToList();
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

        var exercise = new Databases.Journal.Tables.Exercise.Table
        {
            Id = Guid.NewGuid(),
            Name = payload.Name,
            Description = payload.Description,
            Type = payload.Type,
            CreatedDate = DateTime.UtcNow,
            LastUpdated = DateTime.UtcNow
        };
        _context.Exercises.Add(exercise);
        await _context.SaveChangesAsync();
        await _messageBus.PublishAsync(new Post.Messager.Message(exercise.Id));
        await _hubContext.Clients.All.SendAsync("exercise-created", exercise.Id);
        return CreatedAtAction(nameof(Get), exercise.Id);
    }

    [HttpPut]

    public async Task<IActionResult> Put([FromBody] Update.Payload payload)
    {
        if (User.Identity is null)
            return Unauthorized();

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId is null)
            return Unauthorized("User Id not found");

        var exercise = await _context.Exercises.FindAsync(payload.Id);
        if (exercise == null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Exercise not found",
                Detail = $"Exercise with ID {payload.Id} does not exist.",
                Status = StatusCodes.Status404NotFound,
                Instance = HttpContext.Request.Path
            });
        }

        exercise.Name = payload.Name;
        exercise.Description = payload.Description;
        exercise.Type = payload.Type;
        exercise.LastUpdated = DateTime.UtcNow;
        _context.Exercises.Update(exercise);
        await _context.SaveChangesAsync();
        await _messageBus.PublishAsync(new Update.Messager.Message(payload.Id));
        await _hubContext.Clients.All.SendAsync("exercise-updated", payload.Id);
        return NoContent();
    }

    [HttpPatch]
    public async Task<IActionResult> Patch([FromQuery] Guid id,
                                           [FromBody] JsonPatchDocument<Databases.Journal.Tables.Exercise.Table> patchDoc,
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

        var entity = await _context.Exercises.FindAsync(id, cancellationToken);
        if (entity == null)
            return NotFound(new ProblemDetails
            {
                Title = "Exercise not found",
                Detail = $"Exercise with ID {id} does not exist.",
                Status = StatusCodes.Status404NotFound,
                Instance = HttpContext.Request.Path
            });

        patchDoc.ApplyTo(entity);

        entity.LastUpdated = DateTime.UtcNow;

        _context.Exercises.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
        await _hubContext.Clients.All.SendAsync("exercise-updated", entity.Id);

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

        var exercise = await _context.Exercises.FindAsync(parameters.Id);
        if (exercise == null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Exercise not found",
                Detail = $"Exercise with ID {parameters.Id} does not exist.",
                Status = StatusCodes.Status404NotFound,
                Instance = HttpContext.Request.Path
            });
        }

        _context.Exercises.Remove(exercise);
        await _context.SaveChangesAsync();
        await _messageBus.PublishAsync(new Delete.Messager.Message(parameters.Id));
        await _hubContext.Clients.All.SendAsync("exercise-deleted", parameters.Id);
        return NoContent();
    }
}
