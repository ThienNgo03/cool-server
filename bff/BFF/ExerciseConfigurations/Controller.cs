using BFF.Databases.App;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using Wolverine;

namespace BFF.ExerciseConfigurations
{
    [Route("api/exercise-configs")]
    //[Authorize]
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
        [HttpPost("save")]

        public async Task<IActionResult> Save([FromBody] Save.Payload payload)
        {
            //if (User.Identity is null)
            //    return Unauthorized();

            //var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            //if (userId is null)
            //    return Unauthorized("User Id not found");

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
            await _messageBus.PublishAsync(new Save.Messager.Message(workout.Id, payload.WeekPlans, payload.ExerciseId, payload.UserId));
            //await _hubContext.Clients.All.SendAsync("workout-created", workout.Id);
            return NoContent();
        }

    }
}
