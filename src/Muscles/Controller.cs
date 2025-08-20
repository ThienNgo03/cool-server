using Journal.Models.PaginationResults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Journal.Muscles;

[ApiController]
[Authorize]
[Route("api/muscles")]
public class Controller : ControllerBase
{
    private readonly IMessageBus _messageBus;
    private readonly JournalDbContext _context;
    private readonly ILogger<Controller> _logger;
    private readonly IHubContext<Hub> _hubContext;

    public Controller(IMessageBus messageBus, JournalDbContext context, ILogger<Controller> logger, IHubContext<Hub> hubContext)
    {
        _context = context;
        _logger = logger;
        _messageBus = messageBus;
        _hubContext = hubContext;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] Get.Parameters parameters)
    {
        var query = _context.Muscles.AsQueryable();

        if (parameters.Id.HasValue)
            query = query.Where(x => x.Id == parameters.Id);

        if (!string.IsNullOrEmpty(parameters.Name))
            query = query.Where(x => x.Name.Contains(parameters.Name));

        if (parameters.CreatedDate.HasValue)
            query = query.Where(x => x.CreatedDate == parameters.CreatedDate);

        if (parameters.LastUpdated.HasValue)
            query = query.Where(x => x.LastUpdated == parameters.LastUpdated);

        if (parameters.PageSize.HasValue && parameters.PageIndex.HasValue && parameters.PageSize > 0 && parameters.PageIndex >= 0)
            query = query.Skip(parameters.PageIndex.Value * parameters.PageSize.Value).Take(parameters.PageSize.Value);

        var result = await query.AsNoTracking().ToListAsync();

        var paginationResults = new Builder<Databases.Journal.Tables.Muscle.Table>()
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
        var muscle = new Databases.Journal.Tables.Muscle.Table
        {
            Id = Guid.NewGuid(),
            Name = payload.Name,
            CreatedDate = DateTime.UtcNow,
            LastUpdated = DateTime.UtcNow
        };
        _context.Muscles.Add(muscle);
        await _context.SaveChangesAsync();
        await _messageBus.PublishAsync(new Post.Messager.Message(muscle.Id));
        await _hubContext.Clients.All.SendAsync("muscle-created", muscle.Id);
        return CreatedAtAction(nameof(Get), muscle.Id);
    }

    [HttpPut]
    public async Task<IActionResult> Put([FromBody] Update.Payload payload)
    {
        var muscle = await _context.Muscles.FindAsync(payload.Id);
        if (muscle == null)
        {
            return NotFound();
        }

        muscle.Name = payload.Name;
        muscle.LastUpdated = DateTime.UtcNow;
        _context.Muscles.Update(muscle);
        await _context.SaveChangesAsync();
        await _messageBus.PublishAsync(new Update.Messager.Message(payload.Id));
        await _hubContext.Clients.All.SendAsync("muscle-updated", payload.Id);
        return NoContent();
    }

    [HttpDelete]

    public async Task<IActionResult> Delete([FromQuery] Delete.Parameters parameters)
    {
        var muscle = await _context.Muscles.FindAsync(parameters.Id);
        if (muscle == null)
        {
            return NotFound();
        }

        _context.Muscles.Remove(muscle);
        await _context.SaveChangesAsync();
        await _messageBus.PublishAsync(new Delete.Messager.Message(parameters.Id));
        await _hubContext.Clients.All.SendAsync("muscle-deleted", parameters.Id);
        return NoContent();
    }
}
