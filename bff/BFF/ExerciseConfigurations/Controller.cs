using BFF.Databases.App;
using BFF.ExerciseConfigurations.WorkoutLoad;
using BFF.Models.PaginationResults;
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
        private readonly IMessageBus _messageBus;
        private readonly IHubContext<Hub> _hubContext;
        public Controller(JournalDbContext context,
            IMessageBus messageBus,
            IHubContext<Hub> hubContext)
        {
            _context = context;
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
            var existingWorkout = await _context.Workouts
                .Where(w => w.ExerciseId == payload.ExerciseId && w.UserId == payload.UserId && w.Id != workout.Id).Select(w => w.Id).ToListAsync();
            await _messageBus.PublishAsync(new Save.Messager.Message(workout.Id, payload.WeekPlans, payload.ExerciseId, payload.UserId, existingWorkout));
            await _hubContext.Clients.All.SendAsync("workout-created", workout.Id);
            return NoContent();
        }

        [HttpGet("workout-load")]
        public async Task<IActionResult> GetWorkouts([FromQuery] Parameters parameters)
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

            var result = await query.AsNoTracking().ToListAsync();

            // Initialize responses list outside the if condition
            List<Response> responses = result.Select(item => new WorkoutLoad.Response
            {
                Id = item.Id,
                ExerciseId = item.ExerciseId,
                UserId = item.UserId,
                CreatedDate = item.CreatedDate,
                LastUpdated = item.LastUpdated,
                Exercise = null,
                WeekPlans = null
            }).ToList();

            // Dynamically apply Include for valid navigation properties
            Dictionary<Guid, Exercise> exercisesByExerciseId = new();
            var exerciseIds = result.Select(w => w.ExerciseId).Distinct().ToList();
            var exercises = await _context.Exercises
                .Where(e => exerciseIds.Contains(e.Id))
                .ToListAsync();

            // Fetch exercise muscles 
            Dictionary<Guid, List<Muscle>> musclesByExerciseId = new();
            var exerciseMuscleRelations = await _context.ExerciseMuscles
                    .Where(em => exerciseIds.Contains(em.ExerciseId))
                    .ToListAsync();

            var muscleIds = exerciseMuscleRelations.Select(em => em.MuscleId).Distinct().ToList();
            var muscles = await _context.Muscles
                .Where(m => muscleIds.Contains(m.Id))
                .ToDictionaryAsync(m => m.Id);
            // Group muscles by exercise ID
            foreach (var relation in exerciseMuscleRelations)
            {
                if (!musclesByExerciseId.ContainsKey(relation.ExerciseId))
                    musclesByExerciseId[relation.ExerciseId] = new List<Muscle>();

                if (muscles.TryGetValue(relation.MuscleId, out var muscle))
                {
                    musclesByExerciseId[relation.ExerciseId].Add(new WorkoutLoad.Muscle
                    {
                        Id = muscle.Id,
                        Name = muscle.Name,
                        CreatedDate = muscle.CreatedDate,
                        LastUpdated = muscle.LastUpdated
                    });
                }
            }

            // Create exercise models with their muscles
            foreach (var exercise in exercises)
            {
                exercisesByExerciseId[exercise.Id] = new WorkoutLoad.Exercise
                {
                    Id = exercise.Id,
                    Name = exercise.Name,
                    Description = exercise.Description,
                    Type = exercise.Type,
                    CreatedDate = exercise.CreatedDate,
                    LastUpdated = exercise.LastUpdated,
                    Muscles = musclesByExerciseId.ContainsKey(exercise.Id)
                             ? musclesByExerciseId[exercise.Id]
                             : null
                };
            }

            // Update responses with exercises
            foreach (var response in responses)
            {
                if (exercisesByExerciseId.ContainsKey(response.ExerciseId))
                {
                    response.Exercise = exercisesByExerciseId[response.ExerciseId];
                }
            }

            Dictionary<Guid, List<WeekPlan>> workoutWeekPlans = new();
            var workoutIds = result.Select(w => w.Id).ToList();
            var weekPlans = await _context.WeekPlans
                .Where(wp => workoutIds.Contains(wp.WorkoutId))
                .ToListAsync();

            // Fetch WeekPlanSets 
            Dictionary<Guid, List<WeekPlanSet>> weekPlanSetsByWeekPlanId = new();
            var weekPlanIds = weekPlans.Select(wp => wp.Id).ToList();
            var weekPlanSets = await _context.WeekPlanSets
                        .Where(wps => weekPlanIds.Contains(wps.WeekPlanId))
                        .ToListAsync();

            foreach (var set in weekPlanSets)
            {
                if (!weekPlanSetsByWeekPlanId.ContainsKey(set.WeekPlanId))
                    weekPlanSetsByWeekPlanId[set.WeekPlanId] = new List<WeekPlanSet>();

                weekPlanSetsByWeekPlanId[set.WeekPlanId].Add(new WorkoutLoad.WeekPlanSet
                {
                    Id = set.Id,
                    Value = set.Value,
                    WeekPlanId = set.WeekPlanId,
                    InsertedBy = set.InsertedBy,
                    UpdatedBy = set.UpdatedBy,
                    LastUpdated = set.LastUpdated,
                    CreatedDate = set.CreatedDate
                });
            }
            // Group week plans by workout ID
            foreach (var weekPlan in weekPlans)
            {
                if (!workoutWeekPlans.ContainsKey(weekPlan.WorkoutId))
                    workoutWeekPlans[weekPlan.WorkoutId] = new List<WeekPlan>();

                var weekPlanModel = new WorkoutLoad.WeekPlan
                {
                    Id = weekPlan.Id,
                    DateOfWeek = weekPlan.DateOfWeek,
                    Time = weekPlan.Time,
                    WorkoutId = weekPlan.WorkoutId,
                    CreatedDate = weekPlan.CreatedDate,
                    LastUpdated = weekPlan.LastUpdated,
                    WeekPlanSets = weekPlanSetsByWeekPlanId.ContainsKey(weekPlan.Id)
                                  ? weekPlanSetsByWeekPlanId[weekPlan.Id]
                                  : null
                };
                workoutWeekPlans[weekPlan.WorkoutId].Add(weekPlanModel);
            }
            // Update responses with week plans
            foreach (var response in responses)
            {
                if (workoutWeekPlans.ContainsKey(response.Id))
                {
                    response.WeekPlans = workoutWeekPlans[response.Id];
                }
            }

            var paginationResults = new Builder<Response>()
                .WithIndex(parameters.PageIndex)
                .WithSize(parameters.PageSize)
                .WithTotal(responses.Count)
                .WithItems(responses)
                .Build();

            return Ok(paginationResults);
        }
    }
}
