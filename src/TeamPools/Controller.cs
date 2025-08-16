using Journal.Models.PaginationResults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Journal.TeamPools;

[Route("api/team-pools")]
[ApiController]
[Authorize]
public class Controller : ControllerBase
{
    private readonly JournalDbContext _dbContext;
    private readonly IMessageBus _messageBus;
    public Controller(JournalDbContext dbContext, IMessageBus messageBus)
    {
        _dbContext = dbContext;
        _messageBus = messageBus;
    }
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] Get.Parameters parameters)
    {
        var query = _dbContext.TeamPools.AsQueryable();
        // Filtering
        if (parameters.Id.HasValue)
        {
            query = query.Where(x => x.Id == parameters.Id.Value);
        }
        if (parameters.Position.HasValue)
        {
            query = query.Where(x => x.Position == parameters.Position.Value);
        }
        if (parameters.ParticipantId.HasValue)
        {
            query = query.Where(x => x.ParticipantId == parameters.ParticipantId.Value);
        }
        if (parameters.CompetitionId.HasValue)
        {
            query = query.Where(x => x.CompetitionId == parameters.CompetitionId.Value);
        }
        if (parameters.CreatedDate.HasValue)
        {
            query = query.Where(x => x.CreatedDate.Date == parameters.CreatedDate.Value.Date);
        }
        // Pagination
        if (parameters.PageSize.HasValue && parameters.PageIndex.HasValue && parameters.PageSize > 0 && parameters.PageIndex >= 0)
        {
            query = query.Skip(parameters.PageIndex.Value * parameters.PageSize.Value).Take(parameters.PageSize.Value);
        }
        var result = await query.AsNoTracking().ToListAsync();

        var paginationResults = new Builder<Databases.Journal.Tables.TeamPool.Table>()
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
        var existingCompetition = await _dbContext.Competitions.FindAsync(payload.CompetitionId);
        if (existingCompetition == null)
        {
            return NotFound($"Competition with ID {payload.CompetitionId} not found.");
        }
        var teamPool = new Journal.Databases.Journal.Tables.TeamPool.Table
        {
            Id = Guid.NewGuid(),
            Position = payload.Position,
            ParticipantId = payload.ParticipantId,
            CompetitionId = payload.CompetitionId,
            CreatedDate = DateTime.UtcNow
        };
        _dbContext.TeamPools.Add(teamPool);
        await _dbContext.SaveChangesAsync();
        await _messageBus.PublishAsync(new Post.Messager.Message(teamPool.Id));
        return CreatedAtAction(nameof(Get), new { id = teamPool.Id });
    }
    [HttpPut]
    public async Task<IActionResult> Put([FromBody] Put.Payload payload)
    {
        var teamPool = await _dbContext.TeamPools.FindAsync(payload.Id);
        if (teamPool == null)
        {
            return NotFound();
        }
        var existingCompetition = await _dbContext.Competitions.FindAsync(payload.CompetitionId);
        if (existingCompetition == null)
        {
            return NotFound($"Competition with ID {payload.CompetitionId} not found.");
        }

        teamPool.ParticipantId = payload.ParticipantId;
        teamPool.Position = payload.Position;
        teamPool.CompetitionId = payload.CompetitionId;
        _dbContext.TeamPools.Update(teamPool);
        await _dbContext.SaveChangesAsync();
        await _messageBus.PublishAsync(new Put.Messager.Message(teamPool.Id));
        return NoContent();
    }
    [HttpDelete]
    public async Task<IActionResult> Delete([FromQuery] Delete.Parameters parameters)
    {
        var teamPool = await _dbContext.TeamPools.FindAsync(parameters.Id);
        if (teamPool == null)
        {
            return NotFound();
        }
        _dbContext.TeamPools.Remove(teamPool);
        await _dbContext.SaveChangesAsync();
        await _messageBus.PublishAsync(new Delete.Messager.Message(teamPool.Id));
        return NoContent();
    }
}
