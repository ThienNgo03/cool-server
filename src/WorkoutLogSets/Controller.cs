using Journal.Models.PaginationResults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Journal.WorkoutLogSets
{
    [ApiController]
    [Authorize]
    [Route("api/workout-log-sets")]
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
            var query = _context.WorkoutLogSets.AsQueryable();

            if (parameters.Id.HasValue)
                query = query.Where(x => x.Id == parameters.Id);

            if (parameters.WorkoutLogId.HasValue)
                query = query.Where(x => x.WorkoutLogId == parameters.WorkoutLogId);

            if (parameters.Value.HasValue)
                query = query.Where(x => x.Value == parameters.Value);

            if (parameters.CreatedDate.HasValue)
                query = query.Where(x => x.CreatedDate == parameters.CreatedDate);

            if (parameters.LastUpdated.HasValue)
                query = query.Where(x => x.LastUpdated == parameters.LastUpdated);

            if (parameters.PageSize.HasValue && parameters.PageIndex.HasValue && parameters.PageSize > 0 && parameters.PageIndex.Value >= 0)
                query = query.Skip(parameters.PageSize.Value * parameters.PageIndex.Value).Take(parameters.PageSize.Value);

            var result = await query.AsNoTracking().ToListAsync();

            var paginationResults = new Builder<Databases.Journal.Tables.WorkoutLogSet.Table>()
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
            var existingWorkoutLog = await _context.WorkoutLogs.FindAsync(payload.WorkoutLogId);
            if (existingWorkoutLog == null)
            {
                return NotFound();
            }

            var workoutLogSet = new Databases.Journal.Tables.WorkoutLogSet.Table
            {
                Id = Guid.NewGuid(),
                WorkoutLogId = payload.WorkoutLogId,
                Value = payload.Value,
                CreatedDate = DateTime.UtcNow,
                LastUpdated = DateTime.UtcNow
            };

            _context.WorkoutLogSets.Add(workoutLogSet);
            await _context.SaveChangesAsync();
            await _messageBus.PublishAsync(new Post.Messager.Message(workoutLogSet.Id));
            await _hubContext.Clients.All.SendAsync("workout-log-set-created", workoutLogSet.Id);
            return CreatedAtAction(nameof(Get), workoutLogSet.Id);
        }

        [HttpPut]

        public async Task<IActionResult> Put([FromBody] Update.Payload payload)
        {
            var workoutLogSet = await _context.WorkoutLogSets.FindAsync(payload.Id);
            if (workoutLogSet == null)
            {
                return NotFound();
            }
            var existingWorkoutLog = await _context.WorkoutLogs.FindAsync(payload.WorkoutLogId);
            if (existingWorkoutLog == null)
            {
                return NotFound();
            }

            workoutLogSet.WorkoutLogId = payload.WorkoutLogId;
            workoutLogSet.Value = payload.Value;
            workoutLogSet.LastUpdated = DateTime.UtcNow;
            _context.WorkoutLogSets.Update(workoutLogSet);
            await _context.SaveChangesAsync();
            await _messageBus.PublishAsync(new Update.Messager.Message(payload.Id));
            await _hubContext.Clients.All.SendAsync("workout-log-set-updated", payload.Id);
            return NoContent();
        }

        [HttpDelete]

        public async Task<IActionResult> Delete([FromQuery] Delete.Parameters parameters)
        {
            var workoutLogSet = await _context.WorkoutLogSets.FindAsync(parameters.Id);
            if (workoutLogSet == null)
            {
                return NotFound();
            }

            _context.WorkoutLogSets.Remove(workoutLogSet);
            await _context.SaveChangesAsync();
            await _messageBus.PublishAsync(new Delete.Messager.Message(parameters.Id));
            await _hubContext.Clients.All.SendAsync("workout-log-set-deleted", parameters.Id);
            return NoContent();
        }
    }
}
