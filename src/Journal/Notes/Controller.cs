namespace Journal.Notes
{
    [ApiController]
    [Route("api/notes")]
    public class Controller : ControllerBase
    {
        private readonly IMessageBus _messageBus;

        private readonly ILogger<Controller> _logger;

        private readonly JournalDbContext _context; //biến đại diện cho database

        public Controller(ILogger<Controller> logger, JournalDbContext context, IMessageBus messageBus)
        {
            _logger = logger;
            _context = context; // gán database vào biến(_context) đã tạo
            _messageBus = messageBus;
        }
        [HttpGet]

        public async Task<IActionResult> Get([FromQuery] Get.Parameters parameters)// phải có dấu ? sau mỗi property, cho phép để trống chúng khi Get, nếu không sẽ lỗi
        {
            var query = _context.Notes.AsQueryable(); //lấy table ra, nhưng chưa đâm xuống Database
            var all = query;

            if (!string.IsNullOrEmpty(parameters.Ids))
            {
                var ids = parameters.Ids.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(id => Guid.TryParse(id.Trim(), out var guid) ? guid : (Guid?)null)
                .Where(guid => guid.HasValue)
                .Select(guid => guid.Value)
                .ToList();
                query = query.Where(x => ids.Contains(x.Id));
            }
            //Query UserId
            if (parameters.userId.HasValue)
                query = query.Where(x => x.UserId == parameters.userId);
            //Query JourneyId
            if (parameters.journeyId.HasValue)
                query = query.Where(x => x.JourneyId == parameters.journeyId);
            //Query Content
            if (!string.IsNullOrEmpty(parameters.content))
                query = query.Where(x => x.Content.Contains(parameters.content));
            //Query Date
            if (parameters.date.HasValue)
                query = query.Where(x => x.Date == parameters.date);
            //Query Mood
            if (!string.IsNullOrEmpty(parameters.mood))
                query = query.Where(x => x.Mood.Contains(parameters.mood));
            //chia trang
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

            var paginationResults = new Builder<Table>()
              .WithAll(await all.CountAsync())
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
            // check JourneyId
            var journeyId = payload.JourneyId;
            var journey = await _context.Journeys.FirstOrDefaultAsync(x => x.Id == journeyId);
            if (journey == null)
            {
                return NotFound();
            }

            // check UserId
            var userId = payload.UserId;
            var user = await _context.Profiles.FirstOrDefaultAsync(x => x.Id == userId);
            if (user == null)
            {
                return NotFound();
            }

            var note = new Table //tạo một hàng dữ liệu mới
            {
                Id = Guid.NewGuid(),
                UserId = payload.UserId,
                JourneyId = payload.JourneyId,
                Content = payload.Content,
                Date = payload.Date,
                Mood = payload.Mood
            };
            _context.Notes.Add(note); // add hàng dữ liệu mới vào table
            await _context.SaveChangesAsync(); // lưu lại table
            await _messageBus.PublishAsync(new Post.Messager.Message(note.Id));
            return CreatedAtAction(nameof(Get), note.Id);
        }

        [HttpDelete]
        public async Task<IActionResult> Delete([FromQuery] Delete.Parameters parameters) // bắt buộc phải có id để tìm nên không cần dấu ?
        {
            var note = await _context.Notes.FindAsync(parameters.Id);// chờ để ASP.NET tìm và lấy ra data với id được cho
            if (note == null) //nếu data vẫn là null sau khi tìm nghĩa là không có data với id được cho
            {
                return NotFound();
            }
            _context.Notes.Remove(note); //xóa data tìm được khỏi table hiện tại
            await _context.SaveChangesAsync();
            await _messageBus.PublishAsync(new Delete.Messager.Message(parameters.Id));
            return NoContent(); //201
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromBody] Update.Payload payload)
        {
            // check JourneyId
            var journeyId = payload.JourneyId;
            var journey = await _context.Journeys.FirstOrDefaultAsync(x => x.Id == journeyId);
            if (journey == null)
            {
                return NotFound();
            }

            // check UserId
            var userId = payload.UserId;
            var user = await _context.Profiles.FirstOrDefaultAsync(x => x.Id == userId);
            if (user == null)
            {
                return NotFound();
            }

            var note = await _context.Notes.FindAsync(payload.Id);
            if (note == null)
            {
                return NotFound();
            }
            note.Content = payload.Content; // cập nhật data cũ với data mà người dùng nhập vào
            note.Date = payload.Date;
            note.Mood = payload.Mood;
            note.JourneyId = payload.JourneyId;
            note.UserId = payload.UserId;
            _context.Notes.Update(note);
            await _context.SaveChangesAsync();
            await _messageBus.PublishAsync(new Update.Messager.Message(payload.Id));
            return NoContent(); //201
        }
    }
}
