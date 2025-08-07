using Journal.Competitions.Post;
using Microsoft.AspNetCore.SignalR;

namespace Journal.Competitions
{
    [Route("api/competitions")]
    [ApiController]
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
            if (parameters.ParticipantIds != null && parameters.ParticipantIds.Count != 0)
            {
                query = query.Where(c => c.ParticipantIds.Any(id => parameters.ParticipantIds.Contains(id)));
            }
            if( parameters.ExerciseId.HasValue)
            {
                query = query.Where(c => c.ExerciseId == parameters.ExerciseId.Value);
            }
            if (parameters.PageSize.HasValue && parameters.PageIndex.HasValue && parameters.PageSize > 0 && parameters.PageIndex >= 0)
            {
                query = query.Skip(parameters.PageIndex.Value * parameters.PageSize.Value).Take(parameters.PageSize.Value);
            }

            var queryList = await query.AsNoTracking().ToListAsync();

            return Ok(query);
        }
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Payload competition)
        {
            if(competition.Type!= Post.Type.Solo.ToString() && 
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
            _dbContext.Competitions.Add(newCompetition);
            await _dbContext.SaveChangesAsync();
            await _messageBus.PublishAsync(new Post.Messager.Message(newCompetition.Id));
            await _hubContext.Clients.All.SendAsync("competition-created", newCompetition.Id);
            return CreatedAtAction(nameof(Get), new { id = newCompetition.Id });
        }
        [HttpDelete]
        public async Task<IActionResult> Delete([FromQuery] Delete.Parameters parameters)
        {
            var competition = await _dbContext.Competitions.FindAsync(parameters.Id);
            if (competition == null)
            {
                _logger.LogWarning("Attempted to delete a non-existing competition with ID: {Id}", parameters.Id);
                return NotFound();
            }

            _dbContext.Competitions.Remove(competition);
            await _dbContext.SaveChangesAsync();
            await _messageBus.PublishAsync(new Delete.Messager.Message(competition.Id, parameters.DeleteSoloPool, parameters.DeleteTeamPool));    
            return NoContent();
        }
        [HttpPut]
        public async Task<IActionResult> Update([FromBody]Put.Payload competition)
        {
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
            _dbContext.Competitions.Update(existingCompetition);
            await _dbContext.SaveChangesAsync();
            await _messageBus.PublishAsync(new Put.Messager.Message(existingCompetition.Id));
            return NoContent();
        }
    }
}
