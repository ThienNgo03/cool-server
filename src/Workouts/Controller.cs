using Journal.Models.PaginationResults;
using Microsoft.AspNetCore.Authorization;
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

        var QueryResult = await query.AsNoTracking().ToListAsync();
        List<Get.Response> responses = new();
        foreach (var item in QueryResult)
        {
            var response = new Get.Response
            {
                Id = item.Id,
                ExerciseId = item.ExerciseId,
                UserId = item.UserId,
                CreatedDate = item.CreatedDate,
                LastUpdated = item.LastUpdated
            };
            var exercise = await _context.Exercises.FindAsync(item.ExerciseId);
            if (exercise != null)
            {
                response.Exercise = new Get.Exercise
                {
                    Id = exercise.Id,
                    Name = exercise.Name,
                    Description = exercise.Description
                };
            }
            var weekPlans = await _context.WeekPlans.Where(wp => wp.WorkoutId == item.Id).ToListAsync();
            var weekPlanSets = await _context.WeekPlanSets
                .Where(wps => weekPlans.Select(wp => wp.Id).Contains(wps.WeekPlanId))
                .ToListAsync();
            response.WeekPlans = weekPlans.Select(wp => new Get.WeekPlan
            {
                Id = wp.Id,
            }).ToList();
            response.WeekPlans.Select(wp => wp.WeekPlanSets?.Select(wps=>weekPlanSets)).ToList();
            responses.Add(response);
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
            return NotFound();
        }
        var existingUser = await _context.Users.FindAsync(payload.UserId);
        if (existingUser == null)
        {
            return NotFound();
        }
        var workout = new Databases.Journal.Tables.Workout.Table
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
            return NotFound();
        }
        var existingExercise = await _context.Exercises.FindAsync(payload.ExerciseId);
        if (existingExercise == null)
        {
            return NotFound();
        }
        var existingUser = await _context.Users.FindAsync(payload.UserId);
        if (existingUser == null)
        {
            return NotFound();
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

        var workout = await _context.Workouts.FindAsync(parameters.Id);
        if (workout == null)
        {
            return NotFound();
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
