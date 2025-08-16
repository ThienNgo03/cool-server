using Journal.Models.PaginationResults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Hosting;
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

    public Controller(IMessageBus messageBus, ILogger<Controller> logger, JournalDbContext context)
    {
        _messageBus = messageBus;
        _logger = logger;
        _context = context;
    }
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] Get.Parameters parameters)
    {
        var query = _context.MeetUps.AsQueryable();

        if (parameters.Id.HasValue)
            query = query.Where(x => x.Id == parameters.Id);
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

        if (parameters.PageSize.HasValue && parameters.PageIndex.HasValue && parameters.PageSize > 0 && parameters.PageIndex.Value >= 0)
            query = query.Skip(parameters.PageSize.Value * parameters.PageIndex.Value).Take(parameters.PageSize.Value);

        var result = await query.AsNoTracking().ToListAsync();

        var paginationResults = new Builder<Databases.Journal.Tables.MeetUp.Table>()
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
        var meetUp = new Databases.Journal.Tables.MeetUp.Table
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
        return CreatedAtAction(nameof(Get), meetUp.Id);
    }
    [HttpPut]
    public async Task<IActionResult> Put([FromBody] Update.Payload payload)
    {
        var meetUp = await _context.MeetUps.FindAsync(payload.Id);
        if (meetUp == null)
        {
            return NotFound();
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
        return NoContent();
    }

    [HttpDelete]

    public async Task<IActionResult> Delete([FromQuery] Delete.Parameters parameters)
    {
        var meetUp = await _context.MeetUps.FindAsync(parameters.Id);
        if (meetUp == null)
        {
            return NotFound();
        }

        _context.MeetUps.Remove(meetUp);
        await _context.SaveChangesAsync();
        await _messageBus.PublishAsync(new Delete.Messager.Message(parameters.Id));
        return NoContent();
    }
}
