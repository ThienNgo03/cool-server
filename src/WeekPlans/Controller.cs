namespace Journal.WeekPlans;

[ApiController]
[Authorize]
[Route("api/week-plans")]
public class Controller : ControllerBase
{
    private readonly IMessageBus _messageBus;
    private readonly ILogger<Controller> _logger;
    private readonly JournalDbContext _context;
    private readonly IHubContext<Hub> _hubContext;

    public Controller(IMessageBus messageBus, ILogger<Controller> logger, JournalDbContext context, IHubContext<Hub> hubContext)
    {
        _messageBus = messageBus;
        _logger = logger;
        _context = context;
        _hubContext = hubContext;
    }

    [HttpGet]

    public async Task<IActionResult> Get([FromQuery] Get.Parameters parameters)
    {
        var query = _context.WeekPlans.AsQueryable();
        var all = query;
        if (!string.IsNullOrEmpty(parameters.Ids))
        {
            var ids = parameters.Ids.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(id => Guid.TryParse(id.Trim(), out var guid) ? guid : (Guid?)null)
            .Where(guid => guid.HasValue)
            .Select(guid => guid.Value)
            .ToList();
            query = query.Where(x => ids.Contains(x.Id));
        }

        if (parameters.WorkoutId.HasValue)
            query = query.Where(x => x.WorkoutId == parameters.WorkoutId);

        if (!string.IsNullOrEmpty(parameters.DateOfWeek))
            query = query.Where(x => x.DateOfWeek == parameters.DateOfWeek);

        if (parameters.Time.HasValue)
            query = query.Where(x => x.Time == parameters.Time);

        if (parameters.CreatedDate.HasValue)
            query = query.Where(x => x.CreatedDate == parameters.CreatedDate);

        if (parameters.LastUpdated.HasValue)
            query = query.Where(x => x.LastUpdated == parameters.LastUpdated);

        if (!string.IsNullOrEmpty(parameters.SortBy))
        {
            var sortBy = typeof(Table)
                .GetProperties()
                .FirstOrDefault(p => p.Name.Equals(parameters.SortBy, StringComparison.OrdinalIgnoreCase))
                ?.Name;
            if (sortBy != null)
            {
                query = parameters.SortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(x => EF.Property<object>(x, sortBy))
                    : query.OrderBy(x => EF.Property<object>(x, sortBy));
            }
        }

        if (parameters.PageSize.HasValue && parameters.PageIndex.HasValue && parameters.PageSize > 0 && parameters.PageIndex.Value >= 0)
            query = query.Skip(parameters.PageSize.Value * parameters.PageIndex.Value).Take(parameters.PageSize.Value);

        var result = await query.AsNoTracking().ToListAsync();

        var paginationResults = new Builder<Table>()
            .WithAll(await all.CountAsync())
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

        var weekPlan = new Table
        {
            Id = Guid.NewGuid(),
            WorkoutId = payload.WorkoutId,
            DateOfWeek = payload.DateOfWeek,
            Time = payload.Time,
            CreatedDate = DateTime.UtcNow,
            LastUpdated = DateTime.UtcNow
        };

        _context.WeekPlans.Add(weekPlan);
        await _context.SaveChangesAsync();
        await _messageBus.PublishAsync(new Post.Messager.Message(weekPlan.Id));
        await _hubContext.Clients.All.SendAsync("week-plans-created", weekPlan.Id);
        return CreatedAtAction(nameof(Get), weekPlan.Id);
    }

    [HttpPut]

    public async Task<IActionResult> Put([FromBody] Update.Payload payload)
    {
        if (User.Identity is null)
            return Unauthorized();

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId is null)
            return Unauthorized("User Id not found");

        var weekPlan = await _context.WeekPlans.FindAsync(payload.Id);
        if (weekPlan == null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Week Plan not found",
                Detail = $"Week Plan with ID {payload.Id} does not exist.",
                Status = StatusCodes.Status404NotFound,
                Instance = HttpContext.Request.Path
            });
        }

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

        weekPlan.WorkoutId = payload.WorkoutId;
        weekPlan.DateOfWeek = payload.DateOfWeek;
        weekPlan.Time = payload.Time;
        weekPlan.LastUpdated = DateTime.UtcNow;
        _context.WeekPlans.Update(weekPlan);
        await _context.SaveChangesAsync();
        await _messageBus.PublishAsync(new Update.Messager.Message(payload.Id));
        await _hubContext.Clients.All.SendAsync("week-plans-updated", payload.Id);
        return NoContent();
    }

    [HttpPatch]
    public async Task<IActionResult> Patch([FromQuery] Guid id,
                                   [FromBody] JsonPatchDocument<Table> patchDoc,
                                   CancellationToken cancellationToken = default!)
    {
        if (User.Identity is null)
            return Unauthorized();

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId is null)
            return Unauthorized("User Id not found");

        foreach (var op in patchDoc.Operations)
            if (op.OperationType != OperationType.Replace && op.OperationType != OperationType.Test)
                return BadRequest("Only Replace and Test operations are allowed in this patch request.");

        if (patchDoc is null)
            return BadRequest("Patch document cannot be null.");

        var entity = await _context.WeekPlans.FindAsync(id, cancellationToken);
        if (entity == null)
            return NotFound(new ProblemDetails
            {
                Title = "Week Plan not found",
                Detail = $"Week Plan with ID {id} does not exist.",
                Status = StatusCodes.Status404NotFound,
                Instance = HttpContext.Request.Path
            });

        patchDoc.ApplyTo(entity);

        entity.LastUpdated = DateTime.UtcNow;

        _context.WeekPlans.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
        await _hubContext.Clients.All.SendAsync("week-plan-updated", entity.Id);

        return NoContent();
    }

    [HttpDelete]

    public async Task<IActionResult> Delete([FromQuery] Delete.Parameters parameters)
    {
        if (User.Identity is null)
            return Unauthorized();

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId is null)
            return Unauthorized("User Id not found");

        var weekPlan = await _context.WeekPlans.FindAsync(parameters.Id);
        if (weekPlan == null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Week Plan not found",
                Detail = $"Week Plan with ID {parameters.Id} does not exist.",
                Status = StatusCodes.Status404NotFound,
                Instance = HttpContext.Request.Path
            });
        }

        _context.WeekPlans.Remove(weekPlan);
        await _context.SaveChangesAsync();
        await _messageBus.PublishAsync(new Delete.Messager.Message(parameters.Id));
        await _hubContext.Clients.All.SendAsync("week-plans-deleted", parameters.Id);
        return NoContent();
    }
}
