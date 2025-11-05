using Journal.Workouts.Get;
using OpenSearch.Net;
using OpenSearch.Client;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Journal.Exercises;

[ApiController]
[Authorize]
[Route("api/exercises")]
public class Controller(
    IMessageBus messageBus,
    JournalDbContext context,
    ILogger<Controller> logger,
    IHubContext<Hub> hubContext,
    IOpenSearchClient openSearchClient) : ControllerBase
{
    private readonly IMessageBus _messageBus = messageBus;
    private readonly JournalDbContext _context = context;
    private readonly ILogger<Controller> _logger = logger;
    private readonly IHubContext<Hub> _hubContext = hubContext;
    private readonly IOpenSearchClient _openSearchClient = openSearchClient;

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] Get.Parameters parameters)
    {
        var query = _context.Exercises.AsQueryable();

        var all = query;

        List<Guid> ids = new();
        if (!string.IsNullOrEmpty(parameters.SearchTerm))
        {
            var searchResponse = await _openSearchClient.SearchAsync<Databases.OpenSearch.Indexes.Exercise.Index>(s => s
                .Index("exercises")
                .Source(src => src.Includes(i => i.Field(f => f.Id)))
                .Query(q => q
                    .MultiMatch(mm => mm
                        .Query(parameters.SearchTerm)
                        .Fields(f => f
                            .Field(ff => ff.Name)
                            .Field(ff => ff.Description)
                            .Field(ff => ff.Muscles.First().Name)
                            .Field(ff => ff.Type)
                        )
                        .Fuzziness(Fuzziness.Auto)
                    )
                )
            );

            if (!searchResponse.IsValid)
            {
                return StatusCode(500, searchResponse.ServerError?.Error?.Reason ?? searchResponse.DebugInformation);
            }

            ids = searchResponse.Documents.Select(doc => doc.Id).ToList();
        }

        if (!string.IsNullOrEmpty(parameters.Ids))
        {
            var parameterIds = parameters.Ids.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(id => Guid.TryParse(id.Trim(), out var guid) ? guid : (Guid?)null)
                        .Where(guid => guid.HasValue)
                        .Select(guid => guid.Value)
                        .ToList();
            ids = ids.Union(parameterIds).ToList();
        }
        if (ids.Any())
            query = query.Where(x => ids.Contains(x.Id));

        if (!string.IsNullOrEmpty(parameters.Name))
            query = query.Where(x => x.Name.Contains(parameters.Name));

        if (!string.IsNullOrEmpty(parameters.Description))
            query = query.Where(x => x.Description.Contains(parameters.Description));

        if (!string.IsNullOrEmpty(parameters.Type))
            query = query.Where(x => x.Type.Contains(parameters.Type));

        if (parameters.CreatedDate.HasValue)
            query = query.Where(x => x.CreatedDate == parameters.CreatedDate);

        if (parameters.LastUpdated.HasValue)
            query = query.Where(x => x.LastUpdated == parameters.LastUpdated);

        if (parameters.PageSize.HasValue && parameters.PageIndex.HasValue && parameters.PageSize > 0 && parameters.PageIndex >= 0)
            query = query.Skip(parameters.PageIndex.Value * parameters.PageSize.Value).Take(parameters.PageSize.Value);

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

        var result = await query.AsNoTracking().ToListAsync();
        var exerciseIds = result.Select(x => x.Id).ToList();

        var responses = result.Select(exercise => new Get.Response
        {
            Id = exercise.Id,
            Name = exercise.Name,
            Description = exercise.Description,
            Type = exercise.Type,
            CreatedDate = exercise.CreatedDate,
            LastUpdated = exercise.LastUpdated
        }).ToList();

        var paginationResults = new Builder<Get.Response>()
            .WithAll(await all.CountAsync())
            .WithIndex(parameters.PageIndex)
            .WithSize(parameters.PageSize)
            .WithTotal(responses.Count)
            .WithItems(responses)
            .Build();

        if (string.IsNullOrEmpty(parameters.Include))
        {
            return Ok(paginationResults);
        }

        var includes = parameters.Include.Split(',', StringSplitOptions.RemoveEmptyEntries)
                             .Select(i => i.Trim().ToLower())
                             .ToList();

        if (!includes.Any(inc => inc.Split(".")[0] == "muscles") || !exerciseIds.Any())
        {
            return Ok(paginationResults);
        }

        var exerciseMusclesTask = _context.ExerciseMuscles
            .Where(x => exerciseIds.Contains(x.ExerciseId))
            .ToListAsync();

        var exerciseMuscles = await exerciseMusclesTask;
        var muscleIds = exerciseMuscles.Select(x => x.MuscleId).Distinct().ToList();

        if (!muscleIds.Any())
        {
            return Ok(paginationResults);
        }

        var muscles = await _context.Muscles
            .Where(x => muscleIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id);

        var exerciseMuscleGroups = exerciseMuscles
            .GroupBy(x => x.ExerciseId)
            .ToDictionary(g => g.Key, g => g.Select(em => em.MuscleId));

        foreach (var response in responses)
        {
            if (!exerciseMuscleGroups.TryGetValue(response.Id, out var responseMuscleIds))
            {
                continue;
            }

            response.Muscles = responseMuscleIds
                .Where(muscleId => muscles.ContainsKey(muscleId))
                .Select(muscleId => new Get.Muscle
                {
                    Id = muscles[muscleId].Id,
                    Name = muscles[muscleId].Name,
                    CreatedDate = muscles[muscleId].CreatedDate,
                    LastUpdated = muscles[muscleId].LastUpdated
                })
                .ToList();
        }

        if (string.IsNullOrEmpty(parameters.MusclesSortBy))
            return Ok(paginationResults);

        var normalizeProp = typeof(Muscles.Table)
            .GetProperties()
            .FirstOrDefault(p => p.Name.Equals(parameters.MusclesSortBy, StringComparison.OrdinalIgnoreCase))
            ?.Name;

        if (normalizeProp == null)
            return Ok(paginationResults);

        var prop = typeof(Muscles.Table).GetProperty(normalizeProp);
        if (prop == null)
            return Ok(paginationResults);

        var isDescending = parameters.MusclesSortOrder?.ToLower() == "desc";
        foreach (var response in responses.Where(r => r.Muscles?.Any() == true))
        {
            if (response.Muscles == null || !response.Muscles.Any())
                continue;
            response.Muscles = isDescending
                ? response.Muscles.OrderByDescending(m => prop.GetValue(m)).ToList()
                : response.Muscles.OrderBy(m => prop.GetValue(m)).ToList();
        }

        return Ok(paginationResults);
    }

    [HttpPost("sync-open-search")]
    public async Task<IActionResult> SeedingOpenSearch()
    {
        var exercises = await _context.Exercises.AsNoTracking().ToListAsync();
        var exerciseIds = exercises.Select(x => x.Id).ToList();

        if (!exerciseIds.Any())
            return Ok("No exercises to index.");

        var exerciseMuscles = await _context.ExerciseMuscles
            .Where(x => exerciseIds.Contains(x.ExerciseId))
            .ToListAsync();

        var muscleIds = exerciseMuscles.Select(x => x.MuscleId).Distinct().ToList();
        var muscles = await _context.Muscles
            .Where(x => muscleIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id);

        var exerciseMuscleGroups = exerciseMuscles
            .GroupBy(x => x.ExerciseId)
            .ToDictionary(g => g.Key, g => g.Select(em => em.MuscleId).ToList());

        // Create documents to index
        var documentsToIndex = new List<Databases.OpenSearch.Indexes.Exercise.Index>();

        foreach (var exercise in exercises)
        {
            var musclesToIndex = new List<Databases.OpenSearch.Indexes.Muscle.Index>();

            if (exerciseMuscleGroups.TryGetValue(exercise.Id, out var muscleIdsForExercise))
            {
                musclesToIndex = muscleIdsForExercise
                    .Where(muscleId => muscles.ContainsKey(muscleId))
                    .Select(muscleId => new Databases.OpenSearch.Indexes.Muscle.Index
                    {
                        Id = muscles[muscleId].Id,
                        Name = muscles[muscleId].Name,
                        CreatedDate = muscles[muscleId].CreatedDate,
                        LastUpdated = muscles[muscleId].LastUpdated
                    })
                    .ToList();
            }

            documentsToIndex.Add(new Databases.OpenSearch.Indexes.Exercise.Index
            {
                Id = exercise.Id,
                Name = exercise.Name,
                Description = exercise.Description,
                Type = exercise.Type,
                Muscles = musclesToIndex,
                CreatedDate = exercise.CreatedDate,
                LastUpdated = exercise.LastUpdated
            });
        }

        // Bulk index using high-level client
        var bulkResponse = await _openSearchClient.BulkAsync(b => b
            .Index("exercises")
            .IndexMany(documentsToIndex, (descriptor, doc) => descriptor
                .Id(doc.Id.ToString())
                .Document(doc)
            )
        );

        if (!bulkResponse.IsValid)
        {
            return StatusCode(500, $"Bulk indexing failed: {bulkResponse.ServerError?.Error?.Reason ?? bulkResponse.DebugInformation}");
        }

        return Ok($"Successfully indexed {exercises.Count} exercises.");
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] Post.Payload payload)
    {
        if (User.Identity is null)
            return Unauthorized();

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId is null)
            return Unauthorized("User Id not found");

        Table exercise = new()
        {
            Id = Guid.NewGuid(),
            Name = payload.Name,
            Description = payload.Description,
            Type = payload.Type,
            CreatedDate = DateTime.UtcNow,
            LastUpdated = DateTime.UtcNow
        };
        _context.Exercises.Add(exercise);
        await _context.SaveChangesAsync();
        await _messageBus.PublishAsync(new Post.Messager.Message(exercise));
        await _hubContext.Clients.All.SendAsync("exercise-created", exercise.Id);
        return CreatedAtAction(nameof(Get), exercise.Id);
    }

    [HttpPut]

    public async Task<IActionResult> Put([FromBody] Update.Payload payload)
    {
        if (User.Identity is null)
            return Unauthorized();

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId is null)
            return Unauthorized("User Id not found");

        var exercise = await _context.Exercises.FindAsync(payload.Id);
        if (exercise == null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Exercise not found",
                Detail = $"Exercise with ID {payload.Id} does not exist.",
                Status = StatusCodes.Status404NotFound,
                Instance = HttpContext.Request.Path
            });
        }

        exercise.Name = payload.Name;
        exercise.Description = payload.Description;
        exercise.Type = payload.Type;
        exercise.LastUpdated = DateTime.UtcNow;
        _context.Exercises.Update(exercise);
        await _context.SaveChangesAsync();
        await _messageBus.PublishAsync(new Update.Messager.Message(exercise));
        await _hubContext.Clients.All.SendAsync("exercise-updated", payload.Id);
        return NoContent();
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

        var entity = await _context.Exercises.FindAsync(id, cancellationToken);
        if (entity == null)
            return NotFound(new ProblemDetails
            {
                Title = "Exercise not found",
                Detail = $"Exercise with ID {id} does not exist.",
                Status = StatusCodes.Status404NotFound,
                Instance = HttpContext.Request.Path
            });

        patchDoc.ApplyTo(entity);
        //take the column had changed and it value so i can send it to messagebus to sync the data in Opensearch database

        entity.LastUpdated = DateTime.UtcNow;

        _context.Exercises.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
        await _hubContext.Clients.All.SendAsync("exercise-updated", entity.Id);
        await _messageBus.PublishAsync(new Patch.Messager.Message(entity, changes));
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

        var exercise = await _context.Exercises.FindAsync(parameters.Id);
        if (exercise == null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Exercise not found",
                Detail = $"Exercise with ID {parameters.Id} does not exist.",
                Status = StatusCodes.Status404NotFound,
                Instance = HttpContext.Request.Path
            });
        }

        _context.Exercises.Remove(exercise);
        await _context.SaveChangesAsync();
        await _messageBus.PublishAsync(new Delete.Messager.Message(parameters.Id, parameters.IsDeleteWorkouts));
        await _hubContext.Clients.All.SendAsync("exercise-deleted", parameters.Id);
        return NoContent();
    }

}