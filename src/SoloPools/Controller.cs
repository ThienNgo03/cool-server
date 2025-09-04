using Journal.Models.PaginationResults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Journal.SoloPools;

[Route("api/solo-pools")]
[Authorize]
[ApiController]
public class Controller : ControllerBase
{
    #region [ Injection ]
    private readonly JournalDbContext _dbContext;
    private readonly ILogger<Controller> _logger;
    private readonly IMessageBus _messageBus;
    private readonly IHubContext<Hub> _hubContext;
    #endregion

    #region [ CTor ]
    public Controller(JournalDbContext dbContext, ILogger<Controller> logger, IMessageBus messageBus, IHubContext<Hub> hubContext)
    {
        _dbContext = dbContext;
        _logger = logger;
        _messageBus = messageBus;
        _hubContext = hubContext;
    }
    #endregion

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] Get.Parameters parameters)
    {
        var query = _dbContext.SoloPools.AsQueryable();
        // Filtering
        if (parameters.Id.HasValue)
        {
            query = query.Where(x => x.Id == parameters.Id.Value);
        }
        if (parameters.WinnerId.HasValue)
        {
            query = query.Where(x => x.WinnerId == parameters.WinnerId.Value);
        }
        if (parameters.LoserId.HasValue)
        {
            query = query.Where(x => x.LoserId == parameters.LoserId.Value);
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

        var paginationResults = new Builder<Databases.Journal.Tables.SoloPool.Table>()
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
            return NotFound(new ProblemDetails
            {
                Title = "Competition not found",
                Detail = $"Competition with ID {payload.CompetitionId} does not exist.",
                Status = StatusCodes.Status404NotFound,
                Instance = HttpContext.Request.Path
            });
        }

        var refereeId = existingCompetition.RefereeId;
        if (refereeId is null)
            return BadRequest("Require Referee");

        if (refereeId != Guid.Parse(userId))
            return BadRequest("Not match Referee");

        var soloPool = new Databases.Journal.Tables.SoloPool.Table
        {
            Id = Guid.NewGuid(),
            WinnerId = payload.WinnerId,
            LoserId = payload.LoserId,
            CompetitionId = payload.CompetitionId,
            CreatedDate = DateTime.UtcNow
        };

        _dbContext.SoloPools.Add(soloPool);
        await _dbContext.SaveChangesAsync();
        await _messageBus.PublishAsync(new Post.Messager.Message(soloPool.Id));
        await _hubContext.Clients.All.SendAsync("solo-pool-created", soloPool.Id);
        return CreatedAtAction(nameof(Get), new { id = soloPool.Id });
    }
    [HttpPut]
    public async Task<IActionResult> Put([FromBody] Put.Payload payload)
    {
        if (User.Identity is null)
            return Unauthorized();

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId is null)
            return Unauthorized("User Id not found");

        var soloPool = await _dbContext.SoloPools.FindAsync(payload.Id);
        if (soloPool == null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Solo Pool not found",
                Detail = $"Solo Pool with ID {payload.Id} does not exist.",
                Status = StatusCodes.Status404NotFound,
                Instance = HttpContext.Request.Path
            });
        }
        var existingCompetition = await _dbContext.Competitions.FindAsync(payload.CompetitionId);
        if (existingCompetition == null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Competition not found",
                Detail = $"Competition with ID {payload.CompetitionId} does not exist.",
                Status = StatusCodes.Status404NotFound,
                Instance = HttpContext.Request.Path
            });
        }
        soloPool.WinnerId = payload.WinnerId;
        soloPool.LoserId = payload.LoserId;
        soloPool.CompetitionId = payload.CompetitionId;

        _logger.LogInformation("Updating SoloPool with ID: {Id}", payload.Id);

        _dbContext.SoloPools.Update(soloPool);
        await _dbContext.SaveChangesAsync();
        await _messageBus.PublishAsync(new Put.Messager.Message(payload.Id));
        await _hubContext.Clients.All.SendAsync("solo-pool-updated", payload.Id);
        return NoContent();
    }

    [HttpPatch]
    public async Task<IActionResult> Patch([FromQuery] Guid id,
                                       [FromBody] JsonPatchDocument<Databases.Journal.Tables.SoloPool.Table> patchDoc,
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

        var entity = await _dbContext.SoloPools.FindAsync(id, cancellationToken);
        if (entity == null)
            return NotFound(new ProblemDetails
            {
                Title = "Solo Pool not found",
                Detail = $"Solo Pool with ID {id} does not exist.",
                Status = StatusCodes.Status404NotFound,
                Instance = HttpContext.Request.Path
            });

        patchDoc.ApplyTo(entity);

        _dbContext.SoloPools.Update(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await _hubContext.Clients.All.SendAsync("solo-pool-updated", entity.Id);

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

        var soloPool = await _dbContext.SoloPools.FindAsync(parameters.Id);
        if (soloPool == null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Solo Pool not found",
                Detail = $"Solo Pool with ID {parameters.Id} does not exist.",
                Status = StatusCodes.Status404NotFound,
                Instance = HttpContext.Request.Path
            });
        }
        _dbContext.SoloPools.Remove(soloPool);
        await _dbContext.SaveChangesAsync();
        await _messageBus.PublishAsync(new Delete.Messager.Message(parameters.Id));
        await _hubContext.Clients.All.SendAsync("solo-pool-deleted", parameters.Id);
        return NoContent();
    }
}
