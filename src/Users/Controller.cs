namespace Journal.Users;

[ApiController]
[Route("Users")]
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

    public async Task<IActionResult> Get([FromQuery] Get.Parameters parameters)
    {
        var query = _context.Users.AsQueryable();

        if (parameters.id.HasValue)
            query = query.Where(x => x.Id == parameters.id);

        if (!string.IsNullOrEmpty(parameters.name))
            query = query.Where(x => x.Name.Contains(parameters.name));

        if (!string.IsNullOrEmpty(parameters.email))
            query = query.Where(x => x.Email.Contains(parameters.email));

        if (!string.IsNullOrEmpty(parameters.phoneNumber))
            query = query.Where(x => x.PhoneNumber.Contains(parameters.phoneNumber));

        if (parameters.pageSize.HasValue && parameters.pageIndex.HasValue && parameters.pageSize > 0 && parameters.pageIndex >= 0)
            query = query.Skip(parameters.pageIndex.Value * parameters.pageSize.Value).Take(parameters.pageSize.Value);

        var result = await query.AsNoTracking().ToListAsync();
        return Ok(result);

    }

    [HttpPost]

    public async Task<IActionResult> Post([FromBody] Post.Payload payload)
    {
        var user = new Databases.Journal.Tables.User.Table //tạo một hàng dữ liệu mới
        {
            Id = Guid.NewGuid(),
            Name = payload.Name,
            Email = payload.Email,
            PhoneNumber = payload.PhoneNumber
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        await _messageBus.PublishAsync(new Post.Messager.Message(user.Id));
        return CreatedAtAction(nameof(Get), user.Id);
    }

    [HttpPut]

    public async Task<IActionResult> Put([FromBody] Update.Payload payload)
    {
        var user = await _context.Users.FindAsync(payload.Id);
        if (user == null)
        {
            return NotFound();
        }
        user.Name = payload.Name;
        user.PhoneNumber = payload.PhoneNumber;
        user.Email = payload.Email;
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
        await _messageBus.PublishAsync(new Update.Messager.Message(payload.Id));
        return NoContent(); //201
    }

    [HttpDelete]

    public async Task<IActionResult> Delete([FromQuery] Delete.Parameters parameters) // bắt buộc phải có id để tìm nên không cần dấu ?
    {

        var user = await _context.Users.FindAsync(parameters.Id);
        if (user == null)
        {
            return NotFound();
        }
        _context.Users.Remove(user); //xóa data tìm được khỏi table hiện tại
        await _context.SaveChangesAsync();
        await _messageBus.PublishAsync(new Delete.Messager.Message(parameters.Id, parameters.DeleteNotes)); // bắn qua handler
        return NoContent(); //201
    }
}