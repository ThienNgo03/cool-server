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
        List<Get.Response> responses = new();
        int totalCount = 0;

        // ===== MAIN QUERY =====
        var query = _dbContext.Competitions.AsQueryable();
        var all = query;

        // Handle Ids parameter
        if (!string.IsNullOrEmpty(parameters.Ids))
        {
            var ids = parameters.Ids.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(id => Guid.TryParse(id.Trim(), out var guid) ? guid : (Guid?)null)
                .Where(guid => guid.HasValue)
                .Select(guid => guid.Value)
                .ToList();
            query = query.Where(x => ids.Contains(x.Id));
        }

        // Apply filters
        if (parameters.RefereeId.HasValue)
            query = query.Where(c => c.RefereeId == parameters.RefereeId.Value);

        if (!string.IsNullOrEmpty(parameters.Title))
            query = query.Where(c => c.Title.ToLower().Trim().Contains(parameters.Title.ToLower().Trim()));

        if (!string.IsNullOrEmpty(parameters.Description))
            query = query.Where(c => c.Description.ToLower().Trim().Contains(parameters.Description.ToLower().Trim()));

        if (!string.IsNullOrEmpty(parameters.Location))
            query = query.Where(c => c.Location.ToLower().Trim().Contains(parameters.Location.ToLower().Trim()));

        if (parameters.DateTime.HasValue)
            query = query.Where(c => c.DateTime.Date == parameters.DateTime.Value.Date);

        if (parameters.CreatedDate.HasValue)
            query = query.Where(c => c.CreatedDate.Date == parameters.CreatedDate.Value.Date);

        if (!string.IsNullOrEmpty(parameters.Type))
            query = query.Where(c => c.Type.ToLower().Trim() == parameters.Type.ToLower().Trim());

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

        if (parameters.ExerciseId.HasValue)
            query = query.Where(c => c.ExerciseId == parameters.ExerciseId.Value);

        // Apply sorting
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

        // Get total count before pagination
        totalCount = await all.CountAsync();

        // Apply pagination
        if (parameters.PageSize.HasValue && parameters.PageIndex.HasValue && parameters.PageSize > 0 && parameters.PageIndex >= 0)
            query = query.Skip(parameters.PageIndex.Value * parameters.PageSize.Value).Take(parameters.PageSize.Value);

        var result = await query.AsNoTracking().ToListAsync();

        // Build base responses
        responses = result.Select(competition => new Get.Response
        {
            Id = competition.Id,
            RefereeId = competition.RefereeId,
            Title = competition.Title,
            Description = competition.Description,
            Location = competition.Location,
            DateTime = competition.DateTime,
            CreatedDate = competition.CreatedDate,
            Type = competition.Type,
            ParticipantIds = competition.ParticipantIds,
            ExerciseId = competition.ExerciseId,
            SoloPool = null,
            TeamPools = null
        }).ToList();

        Console.WriteLine($"Successfully fetched {responses.Count} competitions from SQL");

        // ===== HANDLE INCLUDE PARAMETER =====
        if (!string.IsNullOrEmpty(parameters.Include))
        {
            var includes = parameters.Include.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                 .Select(i => i.Trim().ToLower())
                                 .ToList();

            var competitionIds = responses.Select(c => c.Id).ToList();

            foreach (var inc in includes)
            {
                if (inc == "solopool")
                {
                    var soloPools = await _dbContext.SoloPools
                        .Where(sp => competitionIds.Contains(sp.CompetitionId))
                        .ToListAsync();

                    var soloPoolDict = soloPools.ToDictionary(sp => sp.CompetitionId);

                    foreach (var response in responses)
                    {
                        if (soloPoolDict.TryGetValue(response.Id, out var soloPool))
                        {
                            response.SoloPool = new Get.SoloPool
                            {
                                Id = soloPool.Id,
                                WinnerId = soloPool.WinnerId,
                                LoserId = soloPool.LoserId,
                                CompetitionId = soloPool.CompetitionId,
                                CreatedDate = soloPool.CreatedDate
                            };
                        }
                    }

                    Console.WriteLine($"Successfully fetched solo pools for {responses.Count} competitions");
                }
                else if (inc == "teampools")
                {
                    var teamPools = await _dbContext.TeamPools
                        .Where(tp => competitionIds.Contains(tp.CompetitionId))
                        .ToListAsync();

                    var teamPoolsByCompetition = teamPools
                        .GroupBy(tp => tp.CompetitionId)
                        .ToDictionary(g => g.Key, g => g.ToList());

                    foreach (var response in responses)
                    {
                        if (teamPoolsByCompetition.TryGetValue(response.Id, out var pools))
                        {
                            response.TeamPools = pools.Select(tp => new Get.TeamPool
                            {
                                Id = tp.Id,
                                ParticipantId = tp.ParticipantId,
                                Position = tp.Position,
                                CompetitionId = tp.CompetitionId,
                                CreatedDate = tp.CreatedDate
                            }).ToList();
                        }
                    }

                    Console.WriteLine($"Successfully fetched team pools for {responses.Count} competitions");
                }
            }
        }

        // ===== BUILD PAGINATION RESULTS =====
        var paginationResults = new Builder<Get.Response>()
            .WithAll(totalCount)
            .WithIndex(parameters.PageIndex)
            .WithSize(parameters.PageSize)
            .WithTotal(responses.Count)
            .WithItems(responses)
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
        Competitions.Table newCompetition = new()
        {
            Id = Guid.NewGuid(),
            Title = competition.Title,
            Description = competition.Description,
            Location = competition.Location,
            DateTime = competition.DateTime,
            CreatedDate = DateTime.UtcNow,
            Type = competition.Type,
            ExerciseId = competition.ExerciseId,
            RefereeId = competition.RefereeId
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
            return NotFound(new ProblemDetails
            {
                Title = "Competition not found",
                Detail = $"Competition with ID {parameters.Id} does not exist.",
                Status = StatusCodes.Status404NotFound,
                Instance = HttpContext.Request.Path
            });
        }

        _dbContext.Competitions.Remove(competition);
        await _dbContext.SaveChangesAsync();
        await _messageBus.PublishAsync(new Delete.Messager.Message(parameters.Id, parameters.DeleteSoloPool, parameters.DeleteTeamPool));
        await _hubContext.Clients.All.SendAsync("competition-deleted", parameters.Id);
        return NoContent();
    }

    [HttpPatch]
    public async Task<IActionResult> Patch([FromQuery] Guid id,
                                           [FromBody] JsonPatchDocument<Competitions.Table> patchDoc,
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
            return NotFound(new ProblemDetails
            {
                Title = "Competition not found",
                Detail = $"Compettion with ID {id} does not exist.",
                Status = StatusCodes.Status404NotFound,
                Instance = HttpContext.Request.Path
            });

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
            return NotFound(new ProblemDetails
            {
                Title = "Competition not found",
                Detail = $"Competition with ID {competition.Id} does not exist.",
                Status = StatusCodes.Status404NotFound,
                Instance = HttpContext.Request.Path
            });
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
        existingCompetition.RefereeId = competition.RefereeId;

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
