namespace Journal.WeekPlans
{
    [ApiController]
    [Route("WeekPlans")]
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
            var query = _context.WeekPlans.AsQueryable();

            if (parameters.Id.HasValue)
                query = query.Where(x => x.Id == parameters.Id);
            if (parameters.WorkoutId.HasValue)
                query = query.Where(x => x.WorkoutId == parameters.WorkoutId);
            if (!string.IsNullOrEmpty(parameters.DateOfWeek))
                query = query.Where(x => x.DateOfWeek == parameters.DateOfWeek);
            if (parameters.Time.HasValue)
                query = query.Where(x => x.Time == parameters.Time);
            if (parameters.Rep.HasValue)
                query = query.Where(x => x.Rep == parameters.Rep);
            if (parameters.HoldingTime.HasValue)
                query = query.Where(x => x.HoldingTime == parameters.HoldingTime);
            if (parameters.Set.HasValue)
                query = query.Where(x => x.Set == parameters.Set);
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
            var weekPlan = new Databases.Journal.Tables.WeekPlan.Table
            {
                Id = Guid.NewGuid(),
                WorkoutId = payload.WorkoutId,
                DateOfWeek = payload.DateOfWeek,
                //DateOfWeek = DateTime.UtcNow.DayOfWeek,
                Time = payload.Time,
                Rep = payload.Rep,
                HoldingTime = payload.HoldingTime,
                Set = payload.Set,
                CreatedDate = DateTime.UtcNow,
                LastUpdated = DateTime.UtcNow
            };

            _context.WeekPlans.Add(weekPlan);
            await _context.SaveChangesAsync();
            await _messageBus.PublishAsync(new Post.Messager.Message(weekPlan.Id));
            return CreatedAtAction(nameof(Get), weekPlan.Id);
        }

        [HttpPut]

        public async Task<IActionResult> Put([FromBody] Update.Payload payload)
        {
            var weekPlan = await _context.WeekPlans.FindAsync(payload.Id);
            if (weekPlan == null)
            {
                return NotFound();
            }

            weekPlan.WorkoutId = payload.WorkoutId;
            weekPlan.DateOfWeek = payload.DateOfWeek;
            //weekPlan.DateOfWeek = DateTime.UtcNow.DayOfWeek;
            weekPlan.Time = payload.Time;
            weekPlan.Rep = payload.Rep;
            weekPlan.HoldingTime = payload.HoldingTime;
            weekPlan.Set = payload.Set;
            weekPlan.LastUpdated = DateTime.UtcNow;
            _context.WeekPlans.Update(weekPlan);
            await _context.SaveChangesAsync();
            await _messageBus.PublishAsync(new Update.Messager.Message(payload.Id));
            return NoContent();
        }

        [HttpDelete]

        public async Task<IActionResult> Delete([FromQuery] Delete.Parameters parameters)
        {
            var weekPlan = await _context.WeekPlans.FindAsync(parameters.Id);
            if (weekPlan == null)
            {
                return NotFound();
            }

            _context.WeekPlans.Remove(weekPlan);
            await _context.SaveChangesAsync();
            await _messageBus.PublishAsync(new Delete.Messager.Message(parameters.Id));
            return NoContent();
        }
    }
}
