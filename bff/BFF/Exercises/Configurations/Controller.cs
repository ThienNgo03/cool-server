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
    [Route("api/exercises/{id}/configs")]
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
        public async Task<IActionResult> Save([FromBody] Save.Payload payload, [FromRoute] Guid? id)
        {
            if (User.Identity is null)
                return Unauthorized();

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId is null)
                return Unauthorized("User Id not found");
            if(id!=null)
                await _workoutInterface.PostAsync(new Library.Workouts.POST.Payload
                {
                    ExerciseId = (Guid)id,
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

            return NoContent();
        }

        [HttpGet("detail")]
        public async Task<IActionResult> Detail([FromQuery] Parameters parameters, [FromRoute] Guid? id)
        {
            if (User.Identity is null)
                return Unauthorized();

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId is null)
                return Unauthorized("User Id not found");

            var list = await _workoutInterface.GetAsync(new()
            {
                ExerciseId = id,
                UserId = parameters.UserId,
                Include = "weekplans.weekplansets",
                PageSize = parameters.PageSize,
                PageIndex = parameters.PageIndex,
                SearchTerm = parameters.SearchTerm,
                CreatedDate = parameters.CreatedDate,
                LastUpdated = parameters.LastUpdated
            });

            var result = list.Items.FirstOrDefault();

            Response response = new Response()
            {
                WorkoutId = result.Id,
                UserId = result.UserId,
                WeekPlans = result.WeekPlans.Select(wp => new WeekPlan()
                {
                    Time = wp.Time,
                    DateOfWeek = wp.DateOfWeek,
                    WeekPlanSets = wp.WeekPlanSets.Select(wps => new WeekPlanSet()
                    {
                        Id = wps.Id,
                        Value = wps.Value
                    }).ToList()
                }).ToList()
            };

            return Ok(response);

        }
    }
}
