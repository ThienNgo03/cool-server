using Journal.Competitions.Post;
using Journal.Models.PaginationResults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Journal.Competitions;

[Route("api/competitions")]
[ApiController]
[Authorize]
public class Controller : ControllerBase
{
    private readonly JournalDbContext _dbContext;
    private readonly IMessageBus _messageBus;
    private readonly ILogger<Controller> _logger;
    private readonly IHubContext<Hub> _hubContext;
    public Controller(JournalDbContext dbContext, 
        ILogger<Controller> logger, 
        IMessageBus messageBus, IHubContext<Hub> hubContext)
    {
        _dbContext = dbContext;
        _logger = logger;
        _messageBus = messageBus;
        _hubContext = hubContext;
    }
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] Get.Parameters parameters)
    {

        var query =_dbContext.Competitions.AsQueryable();

        if (parameters.Id.HasValue)
        {
            query = query.Where(c => c.Id == parameters.Id.Value);
        }
        if (!string.IsNullOrEmpty(parameters.Title))
        {
            query = query.Where(c => c.Title.ToLower().Trim().Contains(parameters.Title.ToLower().Trim()));
        }
        if (!string.IsNullOrEmpty(parameters.Description))
        {
            query = query.Where(c => c.Description.ToLower().Trim().Contains(parameters.Description.ToLower().Trim()));
        }
        if (!string.IsNullOrEmpty(parameters.Location))
        {
            query = query.Where(c => c.Location.ToLower().Trim().Contains(parameters.Location.ToLower().Trim()));
        }
        if (parameters.DateTime.HasValue)
        {
            query = query.Where(c => c.DateTime.Date == parameters.DateTime.Value.Date);
        }
        if (parameters.CreatedDate.HasValue)
        {
            query = query.Where(c => c.CreatedDate.Date == parameters.CreatedDate.Value.Date);
        }
        if (!string.IsNullOrEmpty(parameters.Type))
        {
            query = query.Where(c => c.Type.ToLower().Trim() == parameters.Type.ToLower().Trim());
        }
        if (!string.IsNullOrEmpty(parameters.ParticipantIds))
        {
            var idStrings = parameters.ParticipantIds
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            if (idStrings.Length == 0)
                return BadRequest("ParticipantIds parameter is provided but contains no valid IDs.");

            var guidList = new List<Guid>(idStrings.Length);
            foreach (var idStr in idStrings)
            {
                if (!Guid.TryParse(idStr, out var guid))
                    return BadRequest($"Invalid GUID in ParticipantIds: '{idStr}'");
                guidList.Add(guid);
            }

            query = query.Where(c => c.ParticipantIds.Any(id => guidList.Contains(id)));
        }
        if(parameters.ExerciseId.HasValue)
        {
            query = query.Where(c => c.ExerciseId == parameters.ExerciseId.Value);
        }
        if (parameters.PageSize.HasValue && parameters.PageIndex.HasValue && parameters.PageSize > 0 && parameters.PageIndex >= 0)
        {
            query = query.Skip(parameters.PageIndex.Value * parameters.PageSize.Value).Take(parameters.PageSize.Value);
        }

        var queryResult = await query.AsNoTracking().ToListAsync();

        List<Get.Response> response = new();
        foreach (var item in queryResult) {
            Get.Response competitionResponse = new()
            {
                Id = item.Id,
                Title = item.Title,
                Description = item.Description,
                ParticipantIds = item.ParticipantIds,
                ExerciseId = item.ExerciseId,
                Location = item.Location,
                DateTime = item.DateTime,
                CreatedDate = item.CreatedDate,
                Type = item.Type
            };
            response.Add(competitionResponse);
        }

        if (parameters.IsIncludeSoloPool)
        {
            var soloPool = await _dbContext.SoloPools
                .FirstOrDefaultAsync(x => x.CompetitionId == parameters.Id);

            foreach (var competition in response)
            {
                if (soloPool is null)
                    continue;

                if(competition.Id != soloPool.CompetitionId)
                    continue;

                competition.SoloPool = new Get.SoloPool
                {
                    Id = soloPool.Id,
                    WinnerId = soloPool.WinnerId,
                    LoserId = soloPool.LoserId,
                    CompetitionId = soloPool.CompetitionId,
                    CreatedDate = soloPool.CreatedDate
                };
            }
        }

        if(parameters.IsIncludeTeamPools)
        {
            var teamPools = await _dbContext.TeamPools
                .Where(x => x.CompetitionId == parameters.Id)
                .ToListAsync();

            foreach (var competition in response)
            {                 
                if (teamPools is null || teamPools.Count == 0)
                    continue;
                if(competition.Id != parameters.Id)
                    continue;
                competition.TeamPools = teamPools.Select(tp => new Get.TeamPool
                {
                    Id = tp.Id,
                    ParticipantId = tp.ParticipantId,
                    Position = tp.Position,
                    CompetitionId = tp.CompetitionId,
                    CreatedDate = tp.CreatedDate
                }).ToList();
            }
        }

        var paginationResults = new Builder<Get.Response>()
            .WithIndex(parameters.PageIndex)
            .WithSize(parameters.PageSize)
            .WithTotal(response.Count)
            .WithItems(response)
            .Build();

        return Ok(paginationResults);
    }
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Payload competition)
    {
        if (User.Identity is null)
            return Unauthorized();

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId is null)
            return Unauthorized("User Id not found");

        if (competition.Type!= Post.Type.Solo.ToString() && 
            competition.Type != Post.Type.Team.ToString())
        {
            return BadRequest($"Invalid competition type: {competition.Type}");
        }
        Databases.Journal.Tables.Competition.Table newCompetition = new()
        {
            Id = Guid.NewGuid(),
            Title = competition.Title,
            Description = competition.Description,
            Location = competition.Location,
            DateTime = competition.DateTime,
            CreatedDate = DateTime.UtcNow,
            Type = competition.Type,
            ExerciseId = competition.ExerciseId 
        };
        if(!string.IsNullOrEmpty(competition.ParticipantIds))
        {
            var idStrings = competition.ParticipantIds
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (idStrings.Length == 0)
                return BadRequest("ParticipantIds parameter is provided but contains no valid IDs.");
            var guidList = new List<Guid>(idStrings.Length);
            foreach (var idStr in idStrings)
            {
                if (!Guid.TryParse(idStr, out var guid))
                    return BadRequest($"Invalid GUID in ParticipantIds: '{idStr}'");
                guidList.Add(guid);
            }
            newCompetition.ParticipantIds = guidList;
        }
        _dbContext.Competitions.Add(newCompetition);
        await _dbContext.SaveChangesAsync();
        await _messageBus.PublishAsync(new Post.Messager.Message(newCompetition.Id));
        await _hubContext.Clients.All.SendAsync("competition-created", newCompetition.Id);
        return CreatedAtAction(nameof(Get), new { id = newCompetition.Id });
    }
    [HttpDelete]
    public async Task<IActionResult> Delete([FromQuery] Delete.Parameters parameters)
    {
        if (User.Identity is null)
            return Unauthorized();

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId is null)
            return Unauthorized("User Id not found");

        var competition = await _dbContext.Competitions.FindAsync(parameters.Id);
        if (competition == null)
        {
            _logger.LogWarning("Attempted to delete a non-existing competition with ID: {Id}", parameters.Id);
            return NotFound();
        }

        _dbContext.Competitions.Remove(competition);
        await _dbContext.SaveChangesAsync();
        await _messageBus.PublishAsync(new Delete.Messager.Message(parameters.Id, parameters.DeleteSoloPool, parameters.DeleteTeamPool));
        await _hubContext.Clients.All.SendAsync("competition-deleted", parameters.Id);
        return NoContent();
    }

    [HttpPatch]
    public async Task<IActionResult> Patch([FromQuery] Guid id,
                                           [FromBody] JsonPatchDocument<Databases.Journal.Tables.Competition.Table> patchDoc,
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

        var entity = await _dbContext.Competitions.FindAsync(id, cancellationToken);
        if (entity == null)
            return NotFound();

        patchDoc.ApplyTo(entity);

        _dbContext.Competitions.Update(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await _hubContext.Clients.All.SendAsync("competition-updated", entity.Id);

        return NoContent();
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody]Put.Payload competition)
    {

        if (User.Identity is null)
            return Unauthorized();

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId is null)
            return Unauthorized("User Id not found");

        var existingCompetition = await _dbContext.Competitions.FindAsync(competition.Id);
        if (existingCompetition == null)
        {
            _logger.LogWarning("Attempted to update a non-existing competition with ID: {Id}", competition.Id);
            return NotFound();
        }
        if (competition.Type != Post.Type.Solo.ToString() &&
            competition.Type != Post.Type.Team.ToString())
        {
            return BadRequest($"Invalid competition type: {competition.Type}");
        }
        existingCompetition.Title = competition.Title;
        existingCompetition.Description = competition.Description;
        existingCompetition.Location = competition.Location;
        existingCompetition.DateTime = competition.DateTime;
        existingCompetition.ExerciseId = competition.ExerciseId;
        existingCompetition.Type = competition.Type;

        if (string.IsNullOrEmpty(competition.ParticipantIds))
            existingCompetition.ParticipantIds = new List<Guid>();
        else
        {
            var idStrings = competition.ParticipantIds
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            if (idStrings.Length == 0)
                return BadRequest("ParticipantIds parameter is provided but contains no valid IDs.");

            var guidList = new List<Guid>(idStrings.Length);
            foreach (var idStr in idStrings)
            {
                if (!Guid.TryParse(idStr, out var guid))
                    return BadRequest($"Invalid GUID in ParticipantIds: '{idStr}'");
                guidList.Add(guid);
            }
            existingCompetition.ParticipantIds = guidList;
        }

        _dbContext.Competitions.Update(existingCompetition);
        await _dbContext.SaveChangesAsync();
        await _messageBus.PublishAsync(new Put.Messager.Message(existingCompetition.Id));
        await _hubContext.Clients.All.SendAsync("competition-updated", existingCompetition.Id);
        return NoContent();
    }
}
