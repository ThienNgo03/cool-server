using Journal.Models.PaginationResults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

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
        var query = _context.ExerciseMuscles.AsQueryable();

        if (parameters.Id.HasValue)
            query = query.Where(x => x.Id == parameters.Id);

        if (parameters.ExerciseId.HasValue)
            query = query.Where(x => x.ExerciseId == parameters.ExerciseId);

        if (parameters.MuscleId.HasValue)
            query = query.Where(x => x.MuscleId == parameters.MuscleId);

        if (parameters.CreatedDate.HasValue)
            query = query.Where(x => x.CreatedDate == parameters.CreatedDate);

        if (parameters.LastUpdated.HasValue)
            query = query.Where(x => x.LastUpdated == parameters.LastUpdated);

        if (parameters.PageSize.HasValue && parameters.PageIndex.HasValue && parameters.PageSize > 0 && parameters.PageIndex >= 0)
            query = query.Skip(parameters.PageIndex.Value * parameters.PageSize.Value).Take(parameters.PageSize.Value);

        var result = await query.AsNoTracking().ToListAsync();

        var paginationResults = new Builder<Databases.Journal.Tables.ExerciseMuscle.Table>()
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
            return NotFound($"Exercise with ID {payload.ExerciseId} not found.");
        }

        var existingMuscle = await _context.Muscles.FindAsync(payload.MuscleId);
        if (existingMuscle == null)
        {
            return NotFound($"Muscle with ID {payload.MuscleId} not found.");
        }

        var exerciseMuscle = new Databases.Journal.Tables.ExerciseMuscle.Table
        {
            Id = Guid.NewGuid(),
            ExerciseId = payload.ExerciseId,
            MuscleId = payload.MuscleId,
            CreatedDate = DateTime.UtcNow,
            LastUpdated = DateTime.UtcNow
        };
        _context.ExerciseMuscles.Add(exerciseMuscle);
        await _context.SaveChangesAsync();
        await _messageBus.PublishAsync(new Post.Messager.Message(exerciseMuscle.Id));
        await _hubContext.Clients.All.SendAsync("exercise-muscle-created", exerciseMuscle.Id);
        return CreatedAtAction(nameof(Get), exerciseMuscle.Id);
    }

    [HttpPut]
    public async Task<IActionResult> Put([FromBody] Update.Payload payload)
    {
        var exerciseMuscle = await _context.ExerciseMuscles.FindAsync(payload.Id);
        if (exerciseMuscle == null)
        {
            return NotFound();
        }

        var existingExercise = await _context.Exercises.FindAsync(payload.ExerciseId);
        if (existingExercise == null)
        {
            return NotFound($"Exercise with ID {payload.ExerciseId} not found.");
        }

        var existingMuscle = await _context.Muscles.FindAsync(payload.MuscleId);
        if (existingMuscle == null)
        {
            return NotFound($"Muscle with ID {payload.MuscleId} not found.");
        }

        exerciseMuscle.ExerciseId = payload.ExerciseId;
        exerciseMuscle.MuscleId = payload.MuscleId;
        exerciseMuscle.LastUpdated = DateTime.UtcNow;
        _context.ExerciseMuscles.Update(exerciseMuscle);
        await _context.SaveChangesAsync();
        await _messageBus.PublishAsync(new Update.Messager.Message(payload.Id));
        await _hubContext.Clients.All.SendAsync("exercise-muscle-updated", payload.Id);
        return NoContent();
    }

    [HttpDelete]

    public async Task<IActionResult> Delete([FromQuery] Delete.Parameters parameters)
    {
        var exerciseMuscle = await _context.ExerciseMuscles.FindAsync(parameters.Id);
        if (exerciseMuscle == null)
        {
            return NotFound();
        }

        _context.ExerciseMuscles.Remove(exerciseMuscle);
        await _context.SaveChangesAsync();
        await _messageBus.PublishAsync(new Delete.Messager.Message(parameters.Id));
        await _hubContext.Clients.All.SendAsync("exercise-muscle-deleted", parameters.Id);
        return NoContent();
    }
}
