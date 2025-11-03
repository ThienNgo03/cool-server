using Cassandra.Data.Linq;
using Microsoft.IdentityModel.Tokens;

namespace Journal.ExerciseMuscles;

[ApiController]
[Authorize]
[Route("api/exercise-muscles")]
public class Controller : ControllerBase
{
    private readonly IMessageBus _messageBus;
    private readonly JournalDbContext _context;
    private readonly Databases.CassandraCql.Context _cassandraContext;
    private readonly ILogger<Controller> _logger;
    private readonly IHubContext<Hub> _hubContext;

    public Controller(IMessageBus messageBus, 
        JournalDbContext context, 
        ILogger<Controller> logger, 
        IHubContext<Hub> hubContext,
        Databases.CassandraCql.Context cassandraContext)
    {
        _context = context;
        _logger = logger;
        _messageBus = messageBus;
        _hubContext = hubContext;
        _cassandraContext = cassandraContext;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] Get.Parameters parameters)
    {
        var query = await _cassandraContext.ExerciseMuscles.ExecuteAsync();
        
        if (parameters.PartitionKey.HasValue)
        {
            query = query.Where(x => x.MuscleId == parameters.PartitionKey);
            if (!string.IsNullOrEmpty(parameters.Ids))
            {
                var ids = parameters.Ids.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(id => Guid.TryParse(id.Trim(), out var guid) ? guid : (Guid?)null)
                .Where(guid => guid.HasValue)
                .Select(guid => guid.Value)
                .ToList();
                query = query.Where(x => ids.Contains(x.Id));
            }
        }

        if (parameters.PageSize.HasValue && parameters.PageIndex.HasValue && parameters.PageSize > 0 && parameters.PageIndex >= 0)
            query = query.Skip(parameters.PageIndex.Value * parameters.PageSize.Value).Take(parameters.PageSize.Value);

        var result =  query.ToList();

        var all = (await _cassandraContext.ExerciseMuscles.ExecuteAsync()).Count();

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

        var exerciseMuscle = new Table
        {
            Id = Guid.NewGuid(),
            ExerciseId = payload.ExerciseId,
            MuscleId = payload.MuscleId,
            CreatedDate = DateTime.UtcNow,
            LastUpdated = DateTime.UtcNow
        };
        await _cassandraContext.ExerciseMuscles.Insert(exerciseMuscle).ExecuteAsync();
        await _messageBus.PublishAsync(new Post.Messager.Message(exerciseMuscle));
        await _hubContext.Clients.All.SendAsync("exercise-muscle-created", exerciseMuscle.Id);
        return CreatedAtAction(nameof(Get), exerciseMuscle.Id);
    }

    [HttpPatch]
    public async Task<IActionResult> Patch([FromQuery] Guid id, Guid partitionKey,
                                       [FromBody] JsonPatchDocument<ExerciseMuscles.Table> patchDoc,
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

        var entity = await _cassandraContext.ExerciseMuscles
            .Where(x => x.MuscleId == partitionKey && x.Id == id)
            .FirstOrDefault()
            .ExecuteAsync();
        if (entity == null)
            return NotFound(new ProblemDetails
            {
                Title = "ExerciseMuscle not found",
                Detail = $"ExerciseMuscle with ID {id} does not exist.",
                Status = StatusCodes.Status404NotFound,
                Instance = HttpContext.Request.Path
            });

        patchDoc.ApplyTo(entity);

        entity.LastUpdated = DateTime.UtcNow;

        _context.ExerciseMuscles.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
        await _hubContext.Clients.All.SendAsync("exercise-muscle-updated", entity.Id);
        await _messageBus.PublishAsync(new Patch.Messager.Message(entity, changes));
        return NoContent();
    }

    [HttpPut]
    public async Task<IActionResult> Put([FromBody] Update.Payload payload)
    {
        var exerciseMuscle = await _cassandraContext.ExerciseMuscles
            .Where(x=>x.MuscleId==payload.PartitionKey&&x.Id==payload.Id)
            .FirstOrDefault()
            .ExecuteAsync();
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
        await _cassandraContext.ExerciseMuscles
               .Where(x => x.MuscleId == payload.PartitionKey && x.Id == payload.Id)
               .Delete().ExecuteAsync();

        var newExerciseMuscle = new Table
        {
            Id = payload.Id,
            ExerciseId = payload.NewExerciseId,
            MuscleId = payload.NewMuscleId,
            CreatedDate = exerciseMuscle.CreatedDate,
            LastUpdated = DateTime.UtcNow
        };
        await _cassandraContext.ExerciseMuscles.Insert(newExerciseMuscle).ExecuteAsync();

        await _messageBus.PublishAsync(new Update.Messager.Message(newExerciseMuscle, oldExerciseId, oldMuscleId, payload.NewExerciseId, payload.NewMuscleId));
        await _hubContext.Clients.All.SendAsync("exercise-muscle-updated", payload.Id);
        return NoContent();
    }

    [HttpDelete]

    public async Task<IActionResult> Delete([FromQuery] Delete.Parameters parameters)
    {
        var exerciseMuscle = await _cassandraContext.ExerciseMuscles
            .Where(x => x.MuscleId == parameters.PartitionKey && x.Id == parameters.Id)
            .FirstOrDefault()
            .ExecuteAsync();
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

        await _cassandraContext.ExerciseMuscles
                .Where(x => x.MuscleId == parameters.PartitionKey && x.Id == parameters.Id)
                .Delete().ExecuteAsync();
        await _messageBus.PublishAsync(new Delete.Messager.Message(parameters.Id, exerciseId, muscleId));
        await _hubContext.Clients.All.SendAsync("exercise-muscle-deleted", parameters.Id);
        return NoContent();
    }
}
