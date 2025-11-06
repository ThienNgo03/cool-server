using Cassandra.Data.Linq;
using Journal.ExerciseMuscles.Tables.App;
using Microsoft.IdentityModel.Tokens;

namespace Journal.ExerciseMuscles;

[ApiController]
[Authorize]
[Route("api/exercise-muscles")]
public class Controller : ControllerBase
{
    private readonly IMessageBus _messageBus;
    private readonly JournalDbContext _context;
    private readonly ILogger<Controller> _logger;
    private readonly IHubContext<Hub> _hubContext;

    public Controller(IMessageBus messageBus, 
        JournalDbContext context, 
        ILogger<Controller> logger, 
        IHubContext<Hub> hubContext
        
        )
    {
        _context = context;
        _logger = logger;
        _messageBus = messageBus;
        _hubContext = hubContext;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] Get.Parameters parameters)
    {
        var query = _context.ExerciseMuscles.AsQueryable();
        var all = query.Count();

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
        if (parameters.MuscleId.HasValue)
            query = query.Where(x => x.MuscleId == parameters.MuscleId);

        if (parameters.PageSize.HasValue && parameters.PageIndex.HasValue && parameters.PageSize > 0 && parameters.PageIndex >= 0)
            query = query.Skip(parameters.PageIndex.Value * parameters.PageSize.Value).Take(parameters.PageSize.Value);

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
        var result = await query.AsNoTracking().ToListAsync();


        var paginationResults = new Builder<Table>()
          .WithAll(all)
          .WithIndex(parameters.PageIndex)
          .WithSize(parameters.PageSize)
          .WithTotal(result.Count)
          .WithItems(result)
          .Build();

        return Ok(paginationResults);
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] Post.Payload payload)
    {
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

        var existingMuscle = await _context.Muscles.FindAsync(payload.MuscleId);
        if (existingMuscle == null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Muscle not found",
                Detail = $"Muscle with ID {payload.MuscleId} does not exist.",
                Status = StatusCodes.Status404NotFound,
                Instance = HttpContext.Request.Path
            });
        }

        var exerciseMuscle = new ExerciseMuscles.Tables.App.Table
        {
            Id = Guid.NewGuid(),
            ExerciseId = payload.ExerciseId,
            MuscleId = payload.MuscleId,
            CreatedDate = DateTime.UtcNow,
            LastUpdated = DateTime.UtcNow
        };
        _context.ExerciseMuscles.Add(exerciseMuscle);
        await _context.SaveChangesAsync();
        await _messageBus.PublishAsync(new Post.Messager.Message(exerciseMuscle));
        await _hubContext.Clients.All.SendAsync("exercise-muscle-created", exerciseMuscle.Id);
        return CreatedAtAction(nameof(Get), exerciseMuscle.Id);
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

        var entity = await _context.ExerciseMuscles.FindAsync(id);
        if (entity == null)
            return NotFound(new ProblemDetails
            {
                Title = "ExerciseMuscle not found",
                Detail = $"ExerciseMuscle with ID {id} does not exist.",
                Status = StatusCodes.Status404NotFound,
                Instance = HttpContext.Request.Path
            });

        var oldExerciseId = entity.ExerciseId;
        var oldMuscleId = entity.MuscleId;

        patchDoc.ApplyTo(entity);

        entity.LastUpdated = DateTime.UtcNow;

        _context.ExerciseMuscles.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
        await _hubContext.Clients.All.SendAsync("exercise-muscle-updated", entity.Id);
        await _messageBus.PublishAsync(new Patch.Messager.Message(entity, changes, oldExerciseId, oldMuscleId));
        return NoContent();
    }

    [HttpPut]
    public async Task<IActionResult> Put([FromBody] Update.Payload payload)
    {
        var exerciseMuscle = await _context.ExerciseMuscles.FindAsync(payload.Id);
        if (exerciseMuscle == null)
        {
            return NotFound();
        }
        var oldExerciseId = exerciseMuscle.ExerciseId;
        var oldMuscleId = exerciseMuscle.MuscleId;

        var existingExercise = await _context.Exercises.FindAsync(payload.NewExerciseId);
        if (existingExercise == null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Exercise not found",
                Detail = $"Exercise with ID {payload.NewExerciseId} does not exist.",
                Status = StatusCodes.Status404NotFound,
                Instance = HttpContext.Request.Path
            });
        }

        var existingMuscle = await _context.Muscles.FindAsync(payload.NewMuscleId);
        if (existingMuscle == null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Muscle not found",
                Detail = $"Muscle with ID {payload.NewMuscleId} does not exist.",
                Status = StatusCodes.Status404NotFound,
                Instance = HttpContext.Request.Path
            });
        }
        
        exerciseMuscle.ExerciseId = payload.NewExerciseId;
        exerciseMuscle.MuscleId = payload.NewMuscleId;
        exerciseMuscle.LastUpdated = DateTime.UtcNow;

        _context.ExerciseMuscles.Update(exerciseMuscle);
        await _context.SaveChangesAsync();
        await _messageBus.PublishAsync(new Update.Messager.Message(exerciseMuscle, oldExerciseId, oldMuscleId, payload.NewExerciseId, payload.NewMuscleId));
        await _hubContext.Clients.All.SendAsync("exercise-muscle-updated", payload.Id);
        return NoContent();
    }

    [HttpDelete]

    public async Task<IActionResult> Delete([FromQuery] Delete.Parameters parameters)
    {
        var exerciseMuscle = await _context.ExerciseMuscles.FindAsync(parameters.Id);
        
        if (exerciseMuscle == null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "ExerciseMuscle not found",
                Detail = $"ExerciseMuscle with ID {parameters.Id} does not exist.",
                Status = StatusCodes.Status404NotFound,
                Instance = HttpContext.Request.Path
            });
        }
        var exerciseId = exerciseMuscle.ExerciseId;
        var muscleId = exerciseMuscle.MuscleId;
        _context.ExerciseMuscles.Remove(exerciseMuscle);
        await _context.SaveChangesAsync();
        await _messageBus.PublishAsync(new Delete.Messager.Message(parameters.Id, exerciseId, muscleId));
        await _hubContext.Clients.All.SendAsync("exercise-muscle-deleted", parameters.Id);
        return NoContent();
    }
}
