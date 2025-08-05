﻿namespace Journal.WorkoutLogs
{
    [ApiController]
    [Route("WorkoutLogs")]
    public class Controller: ControllerBase
    {
        private readonly IMessageBus _messageBus;
        private readonly ILogger<Controller> _logger;
        private readonly JournalDbContext _context;
        
        public Controller(IMessageBus messageBus, ILogger<Controller> logger, JournalDbContext context)
        {
            _messageBus = messageBus;
            _logger = logger;
            _context = context;
        }

        [HttpGet]

        public async Task<IActionResult> Get([FromQuery] Get.Parameters parameters)
        {
            var query = _context.WorkoutLogs.AsQueryable();

            if (parameters.Id.HasValue)
                query = query.Where(x => x.Id == parameters.Id);
            if (parameters.WorkoutId.HasValue)
                query = query.Where(x => x.WorkoutId == parameters.WorkoutId);
            if (parameters.Rep.HasValue)
                query = query.Where(x => x.Rep == parameters.Rep);
            if (parameters.HoldingTime.HasValue)
                query = query.Where(x => x.HoldingTime == parameters.HoldingTime);
            if (parameters.Set.HasValue)
                query = query.Where(x => x.Set == parameters.Set);
            if (parameters.WorkoutDate.HasValue)
                query = query.Where(x => x.WorkoutDate == parameters.WorkoutDate);
            if (parameters.CreatedDate.HasValue)
                query = query.Where(x => x.CreatedDate == parameters.CreatedDate);
            if (parameters.LastUpdated.HasValue)
                query = query.Where(x => x.LastUpdated == parameters.LastUpdated);

            if (parameters.PageSize.HasValue && parameters.PageIndex.HasValue && parameters.PageSize > 0 && parameters.PageIndex.Value >= 0)
                query = query.Skip(parameters.PageSize.Value * parameters.PageIndex.Value).Take(parameters.PageSize.Value);

            var result = await query.AsNoTracking().ToListAsync();
            return Ok(result);
        }

        [HttpPost]

        public async Task<IActionResult> Post([FromBody] Post.Payload payload)
        {
            var workoutLog = new Databases.Journal.Tables.WorkoutLog.Table
            {
                Id = Guid.NewGuid(),
                WorkoutId = payload.WorkoutId,
                Rep = payload.Rep,
                HoldingTime = payload.HoldingTime,
                Set = payload.Set,
                WorkoutDate = payload.WorkoutDate,
                CreatedDate = DateTime.UtcNow,
                LastUpdated = DateTime.UtcNow
            };

            _context.WorkoutLogs.Add(workoutLog);
            await _context.SaveChangesAsync();
            await _messageBus.PublishAsync(new Post.Messager.Message(workoutLog.Id));
            return CreatedAtAction(nameof(Get), workoutLog.Id);
        }

        [HttpPut] 

        public async Task<IActionResult> Put([FromBody] Update.Payload payload)
        {
            var workoutLog = await _context.WorkoutLogs.FindAsync(payload.Id);
            if (workoutLog == null)
            {
                return NotFound();
            }

            workoutLog.WorkoutId = payload.WorkoutId;
            workoutLog.Rep = payload.Rep;
            workoutLog.HoldingTime = payload.HoldingTime;
            workoutLog.Set = payload.Set;
            workoutLog.WorkoutDate = payload.WorkoutDate;
            workoutLog.LastUpdated = DateTime.UtcNow;
            _context.WorkoutLogs.Update(workoutLog);
            await _context.SaveChangesAsync();
            await _messageBus.PublishAsync(new Update.Messager.Message(payload.Id));
            return NoContent();
        }

        [HttpDelete]

        public async Task<IActionResult> Delete([FromQuery] Delete.Parameters parameters)
        {
            var workoutLog = await _context.WorkoutLogs.FindAsync(parameters.Id);
            if (workoutLog == null)
            {
                return NotFound();
            }

            _context.WorkoutLogs.Remove(workoutLog);
            await _context.SaveChangesAsync();
            await _messageBus.PublishAsync(new Delete.Messager.Message(parameters.Id));
            return NoContent();
        }
    }
}
