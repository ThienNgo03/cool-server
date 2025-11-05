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

        if (parameters.WeekPlanId.HasValue)
            query = query.Where(x => x.WeekPlanId == parameters.WeekPlanId);

        if (parameters.Value.HasValue)
            query = query.Where(x => x.Value == parameters.Value);

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
            .WithAll(await all.CountAsync())
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
            return NotFound(new ProblemDetails
            {
                Title = "Week Plan not found",
                Detail = $"Week Plan with ID {payload.WeekPlanId} does not exist.",
                Status = StatusCodes.Status404NotFound,
                Instance = HttpContext.Request.Path
            });
        }

        var weekPlanSet = new Table
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
        await _messageBus.PublishAsync(new Post.Messager.Message(weekPlanSet));
        await _hubContext.Clients.All.SendAsync("week-plan-set-created", weekPlanSet.Id);
        return CreatedAtAction(nameof(Get), weekPlanSet.Id);
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

        var changes = new List<(string Path, object? Value)>();
        foreach (var op in patchDoc.Operations)
        {
            if (op.OperationType != OperationType.Replace && op.OperationType != OperationType.Test)
                return BadRequest("Only Replace and Test operations are allowed in this patch request.");
            changes.Add((op.path, op.value));
        }

        if (patchDoc is null)
            return BadRequest("Patch document cannot be null.");

        var entity = await _context.WeekPlanSets.FindAsync(id, cancellationToken);
        if(entity == null)
            return NotFound(new ProblemDetails
            {
                Title = "Week Plan Set not found",
                Detail = $"Week Plan Set with ID {id} does not exist.",
                Status = StatusCodes.Status404NotFound,
                Instance = HttpContext.Request.Path
            });

        patchDoc.ApplyTo(entity);

        entity.LastUpdated = DateTime.UtcNow;
        entity.UpdatedBy = Guid.Parse(userId);

        _context.WeekPlanSets.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
        await _hubContext.Clients.All.SendAsync("week-plan-set-updated", entity.Id);
        await _messageBus.PublishAsync(new Patch.Messager.Message(entity.Id, changes));
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
            return NotFound(new ProblemDetails
            {
                Title = "Week Plan Set not found",
                Detail = $"Week Plan Set with ID {payload.Id} does not exist.",
                Status = StatusCodes.Status404NotFound,
                Instance = HttpContext.Request.Path
            });
        }
        var oldWeekPlanId = weekPlanSet.WeekPlanId;

        var existingWeekPlan = await _context.WeekPlans.FindAsync(payload.WeekPlanId);
        if (existingWeekPlan == null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Week Plan not found",
                Detail = $"Week Plan with ID {payload.WeekPlanId} does not exist.",
                Status = StatusCodes.Status404NotFound,
                Instance = HttpContext.Request.Path
            });
        }
        weekPlanSet.WeekPlanId = payload.WeekPlanId;
        weekPlanSet.Value = payload.Value;
        weekPlanSet.LastUpdated = DateTime.UtcNow;
        weekPlanSet.UpdatedBy = Guid.Parse(userId);
        _context.WeekPlanSets.Update(weekPlanSet);
        await _context.SaveChangesAsync();
        await _messageBus.PublishAsync(new Update.Messager.Message(weekPlanSet, oldWeekPlanId));
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
            return NotFound(new ProblemDetails
            {
                Title = "Week Plan Set not found",
                Detail = $"Week Plan Set with ID {parameters.Id} does not exist.",
                Status = StatusCodes.Status404NotFound,
                Instance = HttpContext.Request.Path
            });
        }
        var weekPlanId = weekPlanSet.WeekPlanId;

        _context.WeekPlanSets.Remove(weekPlanSet);
        await _context.SaveChangesAsync();
        await _messageBus.PublishAsync(new Delete.Messager.Message(parameters.Id, weekPlanId));
        await _hubContext.Clients.All.SendAsync("week-plan-set-deleted", parameters.Id);
        return NoContent();
    }

}
