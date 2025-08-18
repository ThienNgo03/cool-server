using Journal.Models.PaginationResults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Journal.WeekPlanSets;

[ApiController]
[Authorize]
[Route("api/week-plan-sets")]
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
        var query = _context.WeekPlanSets.AsQueryable();

        if (parameters.Id.HasValue)
            query = query.Where(x => x.Id == parameters.Id);

        if (parameters.WeekPlanId.HasValue)
            query = query.Where(x => x.WeekPlanId == parameters.WeekPlanId);

        if (parameters.Value.HasValue)
            query = query.Where(x => x.Value == parameters.Value);

        if (parameters.CreatedDate.HasValue)
            query = query.Where(x => x.CreatedDate == parameters.CreatedDate);

        if (parameters.LastUpdated.HasValue)
            query = query.Where(x => x.LastUpdated == parameters.LastUpdated);

        if (parameters.PageSize.HasValue && parameters.PageIndex.HasValue && parameters.PageSize > 0 && parameters.PageIndex.Value >= 0)
            query = query.Skip(parameters.PageSize.Value * parameters.PageIndex.Value).Take(parameters.PageSize.Value);

        var result = await query.AsNoTracking().ToListAsync();

        var paginationResults = new Builder<Databases.Journal.Tables.WeekPlanSet.Table>()
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
        var existingWeekPlan = await _context.WeekPlans.FindAsync(payload.WeekPlanId);
        if (existingWeekPlan == null)
        {
            return NotFound();
        }

        var weekPlanSet = new Databases.Journal.Tables.WeekPlanSet.Table
        {
            Id = Guid.NewGuid(),
            WeekPlanId = payload.WeekPlanId,
            Value = payload.Value,
            CreatedDate = DateTime.UtcNow,
            LastUpdated = DateTime.UtcNow
        };

        _context.WeekPlanSets.Add(weekPlanSet);
        await _context.SaveChangesAsync();
        await _messageBus.PublishAsync(new Post.Messager.Message(weekPlanSet.Id));
        await _hubContext.Clients.All.SendAsync("week-plan-sets-created", weekPlanSet.Id);
        return CreatedAtAction(nameof(Get), weekPlanSet.Id);
    }

    [HttpPut]

    public async Task<IActionResult> Put([FromBody] Update.Payload payload)
    {
        var weekPlanSet = await _context.WeekPlanSets.FindAsync(payload.Id);
        if (weekPlanSet == null)
        {
            return NotFound();
        }

        var existingWeekPlan = await _context.WeekPlans.FindAsync(payload.WeekPlanId);
        if (existingWeekPlan == null)
        {
            return NotFound();
        }
        weekPlanSet.WeekPlanId = payload.WeekPlanId;
        weekPlanSet.Value = payload.Value;
        weekPlanSet.LastUpdated = DateTime.UtcNow;
        _context.WeekPlanSets.Update(weekPlanSet);
        await _context.SaveChangesAsync();
        await _messageBus.PublishAsync(new Update.Messager.Message(payload.Id));
        await _hubContext.Clients.All.SendAsync("week-plan-sets-updated", payload.Id);
        return NoContent();
    }

    [HttpDelete]

    public async Task<IActionResult> Delete([FromQuery] Delete.Parameters parameters)
    {
        var weekPlanSet = await _context.WeekPlans.FindAsync(parameters.Id);
        if (weekPlanSet == null)
        {
            return NotFound();
        }

        _context.WeekPlans.Remove(weekPlanSet);
        await _context.SaveChangesAsync();
        await _messageBus.PublishAsync(new Delete.Messager.Message(parameters.Id));
        await _hubContext.Clients.All.SendAsync("week-plan-sets-deleted", parameters.Id);
        return NoContent();
    }

}
