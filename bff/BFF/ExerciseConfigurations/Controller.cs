using BFF.Databases.App;
using BFF.ExerciseConfigurations.Detail;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Wolverine;

namespace BFF.ExerciseConfigurations
{
    [Route("api/exercise-configs")]
    [Authorize]
    [ApiController]
    public class Controller : ControllerBase
    {
        private readonly JournalDbContext _context;
        private readonly IMapper _mapper;
        private readonly IMessageBus _messageBus;
        private readonly IHubContext<Hub> _hubContext;
        public Controller(JournalDbContext context,
            IMapper mapper,
            IMessageBus messageBus,
            IHubContext<Hub> hubContext)
        {
            _context = context;
            _mapper = mapper;
            _messageBus = messageBus;
            _hubContext = hubContext;
        }

        [HttpPost("save")]
        public async Task<IActionResult> Save([FromBody] Save.Payload payload)
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
            var oldWorkoutIds = await _context.Workouts
                .Where(w => w.ExerciseId == payload.ExerciseId && w.UserId == payload.UserId && w.Id != workout.Id).Select(w => w.Id).ToListAsync();
            await _messageBus.PublishAsync(new Save.Messager.Message(workout.Id, payload.WeekPlans, payload.ExerciseId, payload.UserId, oldWorkoutIds));
            await _hubContext.Clients.All.SendAsync("saved", workout.Id);
            return CreatedAtAction(nameof(Detail), workout.Id);
        }

        [HttpGet("detail")]
        public async Task<IActionResult> Detail([FromQuery] Parameters parameters)
        {
            var query = _context.Workouts.AsQueryable();

            if (parameters.ExerciseId.HasValue)
                query = query.Where(x => x.ExerciseId == parameters.ExerciseId);
            if (parameters.UserId.HasValue)
                query = query.Where(x => x.UserId == parameters.UserId);
            
            var result = await query.AsNoTracking().FirstOrDefaultAsync();

            if(result is null)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Workout not found",
                    Detail = $"Workout with Exercise ID {parameters.ExerciseId} and User ID {parameters.UserId} does not exist.",
                    Status = StatusCodes.Status404NotFound,
                    Instance = HttpContext.Request.Path
                });
            }

            Response response =  new()
            {
                WorkoutId = result!.Id,
                UserId = result.UserId,
            };

            //PercentageCompletion and Difficulty are not implemented yet

            //Get Exercise details
            var exercise = await _context.Exercises
                .Where(e => e.Id == result.ExerciseId)
                .FirstOrDefaultAsync();
            response.Exercise = new Exercise()
            {
                Id=exercise!.Id,
                Name=exercise!.Name,
            };

            //Get Muscles for the Exercise
            var exerciseMuscles = await _context.ExerciseMuscles
                .Where(m => m.ExerciseId==result.ExerciseId)
                .ToListAsync();
            var muscleIds = exerciseMuscles.Select(em=>em.MuscleId).ToList();
            var muscles = await _context.Muscles
                .Where(m => muscleIds.Contains(m.Id))
                .ToListAsync();
            response.Exercise.Muscles = [.. muscles.Select(m => new Muscle
            {
                Name = m.Name
            })];

            //Get WeekPlans and WeekPlanSets
            var weekPlans =await _context.WeekPlans.Where(w=>w.WorkoutId==result.Id).ToListAsync();
            var weekPlanSets=await _context.WeekPlanSets.Where(ws=>weekPlans.Select(w=>w.Id).Contains(ws.WeekPlanId)).ToListAsync();
            response.WeekPlans = [.. weekPlans.Select(wp => new WeekPlan
            {
                Time = wp.Time,
                DateOfWeek = wp.DateOfWeek,
                WeekPlanSets = [.. weekPlanSets
                .Where(wps => wps.WeekPlanId == wp.Id)
                .Select(wps => new WeekPlanSet
                {
                    Id=wps.Id,
                    Value = wps.Value
                })]
            })];

            response.PercentageCompletion = _mapper.Detail.PercentageCompletion();
            response.Difficulty = _mapper.Detail.Difficulty();

            return Ok(response);
        }
    }
}
