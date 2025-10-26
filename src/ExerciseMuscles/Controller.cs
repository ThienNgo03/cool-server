using Cassandra.Data.Linq;

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
        CqlQuery<Databases.CassandraCql.Tables.ExerciseMuscle.Table> exerciseMuscles = _cassandraContext.ExerciseMuscles;
        var query=await exerciseMuscles.ExecuteAsync();
        
        

        if (parameters.PageSize.HasValue && parameters.PageIndex.HasValue && parameters.PageSize > 0 && parameters.PageIndex >= 0)
            query = query.Skip(parameters.PageIndex.Value * parameters.PageSize.Value).Take(parameters.PageSize.Value);

        var result =  query.ToList();

        var paginationResults = new Builder<Databases.CassandraCql.Tables.ExerciseMuscle.Table>()
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

        var exerciseMuscle = new Databases.CassandraCql.Tables.ExerciseMuscle.Table
        {
            Id = Guid.NewGuid(),
            ExerciseId = payload.ExerciseId,
            MuscleId = payload.MuscleId,
            CreatedDate = DateTime.UtcNow,
            LastUpdated = DateTime.UtcNow
        };
        await _cassandraContext.ExerciseMuscles.Insert(exerciseMuscle).ExecuteAsync();
        await _messageBus.PublishAsync(new Post.Messager.Message(exerciseMuscle.Id, payload.ExerciseId));
        await _hubContext.Clients.All.SendAsync("exercise-muscle-created", exerciseMuscle.Id);
        return CreatedAtAction(nameof(Get), exerciseMuscle.Id);
    }

    [HttpPatch]
    public async Task<IActionResult> Patch([FromQuery] Guid id,
                                       [FromBody] JsonPatchDocument<Databases.App.Tables.ExerciseMuscle.Table> patchDoc,
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

        var entity = await _context.ExerciseMuscles.FindAsync(id, cancellationToken);
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

        return NoContent();
    }

    [HttpPut]
    public async Task<IActionResult> Put([FromBody] Update.Payload payload)
    {
        var exerciseMuscle = await _cassandraContext.ExerciseMuscles
            .Where(x=>x.MuscleId==payload.OldMuscleId&&x.Id==payload.Id)
            .FirstOrDefault()
            .ExecuteAsync();
        if (exerciseMuscle == null)
        {
            return NotFound();
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
        await _cassandraContext.ExerciseMuscles
                .Where(x => x.MuscleId == payload.OldMuscleId && x.Id == payload.Id)
                .Select(x => new Databases.CassandraCql.Tables.ExerciseMuscle.Table
                {
                    ExerciseId= payload.ExerciseId,
                    LastUpdated= DateTime.UtcNow
                })
                .Update().ExecuteAsync();
        await _messageBus.PublishAsync(new Update.Messager.Message(payload.Id, payload.ExerciseId));
        await _hubContext.Clients.All.SendAsync("exercise-muscle-updated", payload.Id);
        return NoContent();
    }

    [HttpDelete]

    public async Task<IActionResult> Delete([FromQuery] Delete.Parameters parameters)
    {
        var exerciseMuscle = await _cassandraContext.ExerciseMuscles
            .Where(x => x.MuscleId == parameters.MuscleId && x.Id == parameters.Id)
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

        await _cassandraContext.ExerciseMuscles
                .Where(x => x.MuscleId == parameters.MuscleId && x.Id == parameters.Id)
                .Delete().ExecuteAsync();
        await _messageBus.PublishAsync(new Delete.Messager.Message(parameters.Id, exerciseId));
        await _hubContext.Clients.All.SendAsync("exercise-muscle-deleted", parameters.Id);
        return NoContent();
    }
}
