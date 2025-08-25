using Journal.Models.PaginationResults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

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

        List<Get.Response> response = result.Select(x => new Get.Response
        {
            Id = x.Id,
            WeekPlanId = x.WeekPlanId,
            Value = x.Value,
            CreatedDate = x.CreatedDate,
            InsertedBy = x.InsertedBy,
            LastUpdated = x.LastUpdated,
            UpdatedBy = x.UpdatedBy
        }).ToList();

        var paginationResults = new Builder<Get.Response>()
            .WithIndex(parameters.PageIndex)
            .WithSize(parameters.PageSize)
            .WithTotal(response.Count)
            .WithItems(response)
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
            InsertedBy = Guid.Parse(userId),
            LastUpdated = DateTime.UtcNow
        };

        _context.WeekPlanSets.Add(weekPlanSet);
        await _context.SaveChangesAsync();
        await _messageBus.PublishAsync(new Post.Messager.Message(weekPlanSet.Id));
        await _hubContext.Clients.All.SendAsync("week-plan-set-created", weekPlanSet.Id);
        return CreatedAtAction(nameof(Get), weekPlanSet.Id);
    }

    [HttpPatch]
    public async Task<IActionResult> Patch([FromQuery] Guid id,
                                           [FromBody] JsonPatchDocument<Databases.Journal.Tables.WeekPlanSet.Table> patchDoc,
                                           CancellationToken cancellationToken = default!)
    {
        if (User.Identity is null)
            return Unauthorized();

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId is null)
            return Unauthorized("User Id not found");

        foreach (var op in patchDoc.Operations)
            if(op.OperationType != OperationType.Replace && op.OperationType != OperationType.Test)
                return BadRequest("Only Replace and Test operations are allowed in this patch request.");

        if(patchDoc is null)
            return BadRequest("Patch document cannot be null.");

        var entity = await _context.WeekPlanSets.FindAsync(id, cancellationToken);
        if(entity == null)
            return NotFound();

        patchDoc.ApplyTo(entity);

        entity.LastUpdated = DateTime.UtcNow;
        entity.UpdatedBy = Guid.Parse(userId);

        _context.WeekPlanSets.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
        await _hubContext.Clients.All.SendAsync("week-plan-set-updated", entity.Id);

        return NoContent();
    }

    [HttpPut]

    public async Task<IActionResult> Put([FromBody] Update.Payload payload)
    {
        if (User.Identity is null)
            return Unauthorized();

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId is null)
            return Unauthorized("User Id not found");

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
        weekPlanSet.UpdatedBy = Guid.Parse(userId);
        _context.WeekPlanSets.Update(weekPlanSet);
        await _context.SaveChangesAsync();
        await _messageBus.PublishAsync(new Update.Messager.Message(payload.Id));
        await _hubContext.Clients.All.SendAsync("week-plan-set-updated", payload.Id);
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

        var weekPlanSet = await _context.WeekPlanSets.FindAsync(parameters.Id);
        if (weekPlanSet == null)
        {
            return NotFound();
        }

        _context.WeekPlanSets.Remove(weekPlanSet);
        await _context.SaveChangesAsync();
        await _messageBus.PublishAsync(new Delete.Messager.Message(parameters.Id));
        await _hubContext.Clients.All.SendAsync("week-plan-set-deleted", parameters.Id);
        return NoContent();
    }

}
