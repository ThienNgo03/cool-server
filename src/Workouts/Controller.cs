namespace Journal.Workouts
{
    [ApiController]
    [Route("Workouts")]
    public class Controller : ControllerBase
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
            return Ok(result);
        }

        [HttpPost]

        public async Task<IActionResult> Post([FromBody] Post.Payload payload)
        {
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
            await _messageBus.PublishAsync(new Post.Messager.Message(workout.Id));
            return CreatedAtAction(nameof(Get), workout.Id);
        }

        [HttpPut]

        public async Task<IActionResult> Put([FromBody] Update.Payload payload)
        {
            var workout = await _context.Workouts.FindAsync(payload.Id);
            if (workout == null)
            {
                return NotFound();
            }

            workout.ExerciseId = payload.ExerciseId;
            workout.UserId = payload.UserId;
            workout.LastUpdated = DateTime.UtcNow;
            _context.Workouts.Update(workout);
            await _context.SaveChangesAsync();
            await _messageBus.PublishAsync(new Update.Messager.Message(payload.Id));
            return NoContent();
        }

        [HttpDelete]

        public async Task<IActionResult> Delete([FromQuery] Delete.Parameters parameters)
        {
            var workout = await _context.Workouts.FindAsync(parameters.Id);
            if (workout == null)
            {
                return NotFound();
            }

            _context.Workouts.Remove(workout);
            await _context.SaveChangesAsync();
            await _messageBus.PublishAsync(new Delete.Messager.Message(parameters.Id));
            return NoContent();
        }
    }
}
