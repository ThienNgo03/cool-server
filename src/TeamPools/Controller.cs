using Journal.Models.PaginationResults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Journal.TeamPools;

[Route("api/team-pools")]
[ApiController]
[Authorize]
public class Controller : ControllerBase
{
    private readonly JournalDbContext _dbContext;
    private readonly IMessageBus _messageBus;
    private readonly IHubContext<Hub> _hubContext;
    public Controller(JournalDbContext dbContext, IMessageBus messageBus, IHubContext<Hub> hubContext)
    {
        _dbContext = dbContext;
        _messageBus = messageBus;
        _hubContext = hubContext;
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
        if (User.Identity is null)
            return Unauthorized();

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId is null)
            return Unauthorized("User Id not found");

        var existingCompetition = await _dbContext.Competitions.FindAsync(payload.CompetitionId);
        if (existingCompetition == null)
        {
            return NotFound($"Competition with ID {payload.CompetitionId} not found.");
        }

        var refereeId = existingCompetition.RefereeId;
        if (refereeId is null)
            return BadRequest("Require Referee");

        if (refereeId != Guid.Parse(userId))
            return BadRequest("Not match Referee");

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
        await _hubContext.Clients.All.SendAsync("team-pool-created", teamPool.Id);
        return CreatedAtAction(nameof(Get), new { id = teamPool.Id });
    }
    [HttpPut]
    public async Task<IActionResult> Put([FromBody] Put.Payload payload)
    {
        if (User.Identity is null)
            return Unauthorized();

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId is null)
            return Unauthorized("User Id not found");

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
        await _messageBus.PublishAsync(new Put.Messager.Message(payload.Id));
        await _hubContext.Clients.All.SendAsync("team-pool-updated", payload.Id);
        return NoContent();
    }

    [HttpPatch]
    public async Task<IActionResult> Patch([FromQuery] Guid id,
                                       [FromBody] JsonPatchDocument<Databases.Journal.Tables.TeamPool.Table> patchDoc,
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

        var entity = await _dbContext.TeamPools.FindAsync(id, cancellationToken);
        if (entity == null)
            return NotFound();

        patchDoc.ApplyTo(entity);

        _dbContext.TeamPools.Update(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await _hubContext.Clients.All.SendAsync("team-pool-updated", entity.Id);

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

        var teamPool = await _dbContext.TeamPools.FindAsync(parameters.Id);
        if (teamPool == null)
        {
            return NotFound();
        }
        _dbContext.TeamPools.Remove(teamPool);
        await _dbContext.SaveChangesAsync();
        await _messageBus.PublishAsync(new Delete.Messager.Message(parameters.Id));
        await _hubContext.Clients.All.SendAsync("team-pool-deleted", parameters.Id);
        return NoContent();
    }
}
