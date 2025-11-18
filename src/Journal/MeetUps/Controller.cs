using Journal.Models.PaginationResults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using System.Security.Claims;
using Wolverine.Persistence;

namespace Journal.MeetUps;

[ApiController]
[Authorize]
[Route("api/meet-ups")]
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

        var query = _context.MeetUps.AsQueryable();
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
        if (!string.IsNullOrEmpty(parameters.ParticipantIds))
            query = query.Where(x => x.ParticipantIds.Contains(parameters.ParticipantIds));
        if (!string.IsNullOrEmpty(parameters.Title))
            query = query.Where(x => x.Title.Contains(parameters.Title));
        if (parameters.DateTime.HasValue)
            query = query.Where(x => x.DateTime == parameters.DateTime);
        if (!string.IsNullOrEmpty(parameters.Location))
            query = query.Where(x => x.Location.Contains(parameters.Location));

        if (!string.IsNullOrEmpty(parameters.CoverImage))
            query = query.Where(x => x.CoverImage.Contains(parameters.CoverImage));

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

        var meetUp = new MeetUps.Table
        {
            Id = Guid.NewGuid(),
            ParticipantIds = payload.ParticipantIds,
            Title = payload.Title,
            DateTime = payload.DateTime,
            Location = payload.Location,
            CoverImage = payload.CoverImage,
            CreatedDate = DateTime.UtcNow,
            LastUpdated = DateTime.UtcNow
        };

        _context.MeetUps.Add(meetUp);
        await _context.SaveChangesAsync();
        await _messageBus.PublishAsync(new Post.Messager.Message(meetUp.Id));
        await _hubContext.Clients.All.SendAsync("meet-up-created", meetUp.Id);
        return CreatedAtAction(nameof(Get), meetUp.Id);
    }
    [HttpPut]
    public async Task<IActionResult> Put([FromBody] Update.Payload payload)
    {
        if (User.Identity is null)
            return Unauthorized();

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId is null)
            return Unauthorized("User Id not found");

        var meetUp = await _context.MeetUps.FindAsync(payload.Id);
        if (meetUp == null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Meet Up not found",
                Detail = $"Meet Up with ID {payload.Id} does not exist.",
                Status = StatusCodes.Status404NotFound,
                Instance = HttpContext.Request.Path
            });
        }

        meetUp.ParticipantIds = payload.ParticipantIds;
        meetUp.Title = payload.Title;
        meetUp.DateTime = payload.DateTime;
        meetUp.Location = payload.Location;
        meetUp.CoverImage = payload.CoverImage;
        meetUp.LastUpdated = DateTime.UtcNow;
        _context.MeetUps.Update(meetUp);
        await _context.SaveChangesAsync();
        await _messageBus.PublishAsync(new Update.Messager.Message(payload.Id));
        await _hubContext.Clients.All.SendAsync("meet-up-updated", payload.Id);
        return NoContent();
    }

    [HttpPatch]
    public async Task<IActionResult> Patch([FromQuery] Guid id,
                                           [FromBody] JsonPatchDocument<MeetUps.Table> patchDoc,
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

        var entity = await _context.MeetUps.FindAsync(id, cancellationToken);
        if (entity == null)
            return NotFound(new ProblemDetails
            {
                Title = "Meet Up not found",
                Detail = $"Meet Up with ID {id} does not exist.",
                Status = StatusCodes.Status404NotFound,
                Instance = HttpContext.Request.Path
            });

        patchDoc.ApplyTo(entity);

        entity.LastUpdated = DateTime.UtcNow;

        _context.MeetUps.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
        await _hubContext.Clients.All.SendAsync("meet-up-updated", entity.Id);

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

        var meetUp = await _context.MeetUps.FindAsync(parameters.Id);
        if (meetUp == null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Meet Up not found",
                Detail = $"Meet Up with ID {parameters.Id} does not exist.",
                Status = StatusCodes.Status404NotFound,
                Instance = HttpContext.Request.Path
            });
        }

        _context.MeetUps.Remove(meetUp);
        await _context.SaveChangesAsync();
        await _messageBus.PublishAsync(new Delete.Messager.Message(parameters.Id));
        await _hubContext.Clients.All.SendAsync("meet-up-deleted", parameters.Id);
        return NoContent();
    }
}
