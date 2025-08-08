using Journal.Models.PaginationResults;

namespace Journal.SoloPools;

[Route("api/solo-pools")]
[ApiController]
public class Controller : ControllerBase
{
    #region [ Injection ]
    private readonly JournalDbContext _dbContext;
    private readonly ILogger<Controller> _logger;
    private readonly IMessageBus _messageBus;
    #endregion

    #region [ CTor ]
    public Controller(JournalDbContext dbContext, ILogger<Controller> logger, IMessageBus messageBus)
    {
        _dbContext = dbContext;
        _logger = logger;
        _messageBus = messageBus;
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
        var existingCompetition = await _dbContext.Competitions.FindAsync(payload.CompetitionId);
        if (existingCompetition == null)
        {
            return NotFound($"Competition with ID {payload.CompetitionId} not found.");
        }
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
        return CreatedAtAction(nameof(Get), new { id = soloPool.Id });
    }
    [HttpPut]
    public async Task<IActionResult> Put([FromBody] Put.Payload payload)
    {   
        var soloPool = await _dbContext.SoloPools.FindAsync(payload.Id);
        if (soloPool == null)
        {
            return NotFound();
        }
        var existingCompetition = await _dbContext.Competitions.FindAsync(payload.CompetitionId);
        if (existingCompetition == null)
        {
            return NotFound($"Competition with ID {payload.CompetitionId} not found.");
        }
        soloPool.WinnerId = payload.WinnerId;
        soloPool.LoserId = payload.LoserId;
        soloPool.CompetitionId = payload.CompetitionId;

        _logger.LogInformation("Updating SoloPool with ID: {Id}", payload.Id);

        _dbContext.SoloPools.Update(soloPool);
        await _dbContext.SaveChangesAsync();
        await _messageBus.PublishAsync(new Put.Messager.Message(soloPool.Id));
        return NoContent();
    }
    [HttpDelete]
    public async Task<IActionResult> Delete([FromQuery] Delete.Parameters parameters)
    {
        var soloPool = await _dbContext.SoloPools.FindAsync(parameters.Id);
        if (soloPool == null)
        {
            return NotFound();
        }
        _dbContext.SoloPools.Remove(soloPool);
        await _dbContext.SaveChangesAsync();
        await _messageBus.PublishAsync(new Delete.Messager.Message(soloPool.Id));
        return NoContent();
    }
}
