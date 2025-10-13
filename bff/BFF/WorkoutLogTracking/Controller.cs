using BFF.Databases.App;
using BFF.Models.PaginationResults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Wolverine;

namespace BFF.WorkoutLogTracking;

[Route("api/workoutLogs-tracking")]
[Authorize]
[ApiController]
public class Controller : ControllerBase
{
    private readonly JournalDbContext _context;
    private readonly IMessageBus _messageBus;
    public Controller(JournalDbContext context,
        IMessageBus messageBus)
    {
        _context = context;
        _messageBus = messageBus;
    }
    [HttpGet("get-workoutLogs")]

    public async Task<IActionResult> GetWorkoutLogs([FromQuery] GetWorkoutLogs.Parameters parameters)
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

    [HttpPost("create-workoutLogs")]

    public async Task<IActionResult> Post([FromBody] CreateWorkoutLogs.Payload payload)
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
        await _messageBus.PublishAsync(new CreateWorkoutLogs.Messager.Message(workoutLog.Id));
        //await _hubContext.Clients.All.SendAsync("workout-log-created", workoutLog.Id);
        return CreatedAtAction(nameof(GetWorkoutLogs), workoutLog.Id);
    }
}
