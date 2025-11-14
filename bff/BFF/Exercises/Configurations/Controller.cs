using BFF.Databases.App;
using BFF.Exercises.Configurations.Detail;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Wolverine;

namespace BFF.Exercises.Configurations
{
    [Route("api/exercises/configs")]
    [Authorize]
    [ApiController]
    public class Controller : ControllerBase
    {
        private readonly JournalDbContext _context;
        private readonly IMapper _mapper;
        private readonly IMessageBus _messageBus;
        private readonly IHubContext<Hub> _hubContext;
        private readonly Library.Workouts.Interface _workoutInterface;
        public Controller(JournalDbContext context,
            IMapper mapper,
            IMessageBus messageBus,
            IHubContext<Hub> hubContext,
            Library.Workouts.Interface workoutInterface)
        {
            _context = context;
            _mapper = mapper;
            _messageBus = messageBus;
            _hubContext = hubContext;
            _workoutInterface = workoutInterface;
        }

        [HttpPost("save")]
        public async Task<IActionResult> Save([FromBody] Save.Payload payload)
        {
            if (User.Identity is null)
                return Unauthorized();

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId is null)
                return Unauthorized("User Id not found");

            await _workoutInterface.PostAsync(new Library.Workouts.POST.Payload
            {
                ExerciseId = payload.ExerciseId,
                UserId = payload.UserId,
                WeekPlans = payload.WeekPlans?.Select(wp => new Library.Workouts.POST.WeekPlan
                {
                    DateOfWeek = wp.DateOfWeek,
                    Time = wp.Time,
                    WeekPlanSets = wp.WeekPlanSets?.Select(wps => new Library.Workouts.POST.WeekPlanSet
                    {
                        Value = wps.Value
                    }).ToList()
                }).ToList()
            });

            return Ok();
        }

        //[HttpGet("detail")]
        //public async Task<IActionResult> Detail([FromQuery] Parameters parameters)
        //{
        //    var query = _context.Workouts.AsQueryable();

        //    if (parameters.ExerciseId.HasValue)
        //        query = query.Where(x => x.ExerciseId == parameters.ExerciseId);
        //    if (parameters.UserId.HasValue)
        //        query = query.Where(x => x.UserId == parameters.UserId);
            
        //    var result = await query.AsNoTracking().FirstOrDefaultAsync();

        //    if(result is null)
        //    {
        //        return NotFound(new ProblemDetails
        //        {
        //            Title = "Workout not found",
        //            Detail = $"Workout with Exercise ID {parameters.ExerciseId} and User ID {parameters.UserId} does not exist.",
        //            Status = StatusCodes.Status404NotFound,
        //            Instance = HttpContext.Request.Path
        //        });
        //    }

        //    Response response =  new()
        //    {
        //        WorkoutId = result!.Id,
        //        UserId = result.UserId,
        //    };

        //    //PercentageCompletion and Difficulty are not implemented yet

        //    //Get Exercise details
        //    var exercise = await _context.Exercises
        //        .Where(e => e.Id == result.ExerciseId)
        //        .FirstOrDefaultAsync();
        //    response.Exercise = new Exercise()
        //    {
        //        Id=exercise!.Id,
        //        Name=exercise!.Name,
        //    };

        //    //Get Muscles for the Exercise
        //    var exerciseMuscles = await _context.ExerciseMuscles
        //        .Where(m => m.ExerciseId==result.ExerciseId)
        //        .ToListAsync();
        //    var muscleIds = exerciseMuscles.Select(em=>em.MuscleId).ToList();
        //    var muscles = await _context.Muscles
        //        .Where(m => muscleIds.Contains(m.Id))
        //        .ToListAsync();
        //    response.Exercise.Muscles = [.. muscles.Select(m => new Muscle
        //    {
        //        Name = m.Name
        //    })];

        //    //Get WeekPlans and WeekPlanSets
        //    var weekPlans =await _context.WeekPlans.Where(w=>w.WorkoutId==result.Id).ToListAsync();
        //    var weekPlanSets=await _context.WeekPlanSets.Where(ws=>weekPlans.Select(w=>w.Id).Contains(ws.WeekPlanId)).ToListAsync();
        //    response.WeekPlans = [.. weekPlans.Select(wp => new WeekPlan
        //    {
        //        Time = wp.Time,
        //        DateOfWeek = wp.DateOfWeek,
        //        WeekPlanSets = [.. weekPlanSets
        //        .Where(wps => wps.WeekPlanId == wp.Id)
        //        .Select(wps => new WeekPlanSet
        //        {
        //            Id=wps.Id,
        //            Value = wps.Value
        //        })]
        //    })];

        //    response.PercentageCompletion = _mapper.Detail.PercentageCompletion();
        //    response.Difficulty = _mapper.Detail.Difficulty();

        //    return Ok(response);
        //}
    }
}
