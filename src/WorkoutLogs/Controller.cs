using Journal.Models.PaginationResults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Journal.WorkoutLogs;

[ApiController]
[Authorize]
[Route("api/workout-logs")]
public class Controller : ControllerBase
{
    private readonly IMessageBus _messageBus;
    private readonly ILogger<Controller> _logger;
    private readonly JournalDbContext _context;
    private readonly IHubContext<Hub> _hubContext;

    public Controller(IMessageBus messageBus,
                      ILogger<Controller> logger,
                      JournalDbContext context,
                      IHubContext<Hub> hubContext)
    {
        _messageBus = messageBus;
        _logger = logger;
        _context = context;
        _hubContext = hubContext;
    }

    [HttpGet]

    public async Task<IActionResult> Get([FromQuery] Get.Parameters parameters)
    {
        var query = _context.WorkoutLogs.AsQueryable();

        if (parameters.Id.HasValue)
            query = query.Where(x => x.Id == parameters.Id);

        if (parameters.WorkoutId.HasValue)
            query = query.Where(x => x.WorkoutId == parameters.WorkoutId);

        if (parameters.WorkoutDate.HasValue)
            query = query.Where(x => x.WorkoutDate == parameters.WorkoutDate);

        if (parameters.CreatedDate.HasValue)
            query = query.Where(x => x.CreatedDate == parameters.CreatedDate);

        if (parameters.LastUpdated.HasValue)
            query = query.Where(x => x.LastUpdated == parameters.LastUpdated);

        if (parameters.PageSize.HasValue && parameters.PageIndex.HasValue && parameters.PageSize > 0 && parameters.PageIndex.Value >= 0)
            query = query.Skip(parameters.PageSize.Value * parameters.PageIndex.Value).Take(parameters.PageSize.Value);

        var result = await query.AsNoTracking().ToListAsync();

        var paginationResults = new Builder<Databases.App.Tables.WorkoutLog.Table>()
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
        if (User.Identity is null)
            return Unauthorized();

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId is null)
            return Unauthorized("User Id not found");

        var existingWorkout = await _context.Workouts.FindAsync(payload.WorkoutId);
        if (existingWorkout == null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Workout not found",
                Detail = $"Workout with ID {payload.WorkoutId} does not exist.",
                Status = StatusCodes.Status404NotFound,
                Instance = HttpContext.Request.Path
            });
        }

        var workoutLog = new Databases.App.Tables.WorkoutLog.Table
        {
            Id = Guid.NewGuid(),
            WorkoutId = payload.WorkoutId,
            WorkoutDate = payload.WorkoutDate,
            CreatedDate = DateTime.UtcNow,
            LastUpdated = DateTime.UtcNow
        };

        _context.WorkoutLogs.Add(workoutLog);
        await _context.SaveChangesAsync();
        await _messageBus.PublishAsync(new Post.Messager.Message(workoutLog.Id));
        await _hubContext.Clients.All.SendAsync("workout-log-created", workoutLog.Id);
        return CreatedAtAction(nameof(Get), workoutLog.Id);
    }

    [HttpPatch]
    public async Task<IActionResult> Patch([FromQuery] Guid id,
                                       [FromBody] JsonPatchDocument<Databases.App.Tables.WorkoutLog.Table> patchDoc,
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

        var entity = await _context.WorkoutLogs.FindAsync(id, cancellationToken);
        if (entity == null)
            return NotFound(new ProblemDetails
            {
                Title = "Workout Log not found",
                Detail = $"Workout Log with ID {id} does not exist.",
                Status = StatusCodes.Status404NotFound,
                Instance = HttpContext.Request.Path
            });

        patchDoc.ApplyTo(entity);

        entity.LastUpdated = DateTime.UtcNow;

        _context.WorkoutLogs.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
        await _hubContext.Clients.All.SendAsync("workout-log-updated", entity.Id);

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

        var workoutLog = await _context.WorkoutLogs.FindAsync(payload.Id);
        if (workoutLog == null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Workout Log not found",
                Detail = $"Workout Log with ID {payload.Id} does not exist.",
                Status = StatusCodes.Status404NotFound,
                Instance = HttpContext.Request.Path
            });
        }
        var existingWorkout = await _context.Workouts.FindAsync(payload.WorkoutId);
        if (existingWorkout == null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Workout not found",
                Detail = $"Workout with ID {payload.WorkoutId} does not exist.",
                Status = StatusCodes.Status404NotFound,
                Instance = HttpContext.Request.Path
            });
        }

        workoutLog.WorkoutId = payload.WorkoutId;
        workoutLog.WorkoutDate = payload.WorkoutDate;
        workoutLog.LastUpdated = DateTime.UtcNow;
        _context.WorkoutLogs.Update(workoutLog);
        await _context.SaveChangesAsync();
        await _messageBus.PublishAsync(new Update.Messager.Message(payload.Id));
        await _hubContext.Clients.All.SendAsync("workout-log-updated", payload.Id);
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

        var workoutLog = await _context.WorkoutLogs.FindAsync(parameters.Id);
        if (workoutLog == null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Workout Log not found",
                Detail = $"Workout Log with ID {parameters.Id} does not exist.",
                Status = StatusCodes.Status404NotFound,
                Instance = HttpContext.Request.Path
            });
        }

        _context.WorkoutLogs.Remove(workoutLog);
        await _context.SaveChangesAsync();
        await _messageBus.PublishAsync(new Delete.Messager.Message(parameters.Id));
        await _hubContext.Clients.All.SendAsync("workout-log-deleted", parameters.Id);
        return NoContent();
    }
}
