using Journal.Models.PaginationResults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Journal.Users;

[ApiController]
[Route("api/users")]
[Authorize]
public class Controller : ControllerBase
{
    private readonly IMessageBus _messageBus;

    private readonly ILogger<Controller> _logger;

    private readonly JournalDbContext _context;
    private readonly IHubContext<Hub> _hubContext;

    public Controller(ILogger<Controller> logger, JournalDbContext context, IMessageBus messageBus, IHubContext<Hub> hubContext)
    {
        _logger = logger;
        _context = context;
        _messageBus = messageBus;
        _hubContext = hubContext;
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

        var paginationResults = new Builder<Databases.Journal.Tables.User.Table>()
                .WithIndex(parameters.pageIndex)
                .WithSize(parameters.pageSize)
                .WithTotal(result.Count)
                .WithItems(result)
                .Build();

        return Ok(paginationResults);

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
            return NotFound(new ProblemDetails
            {
                Title = "User not found",
                Detail = $"User with ID {payload.Id} does not exist.",
                Status = StatusCodes.Status404NotFound,
                Instance = HttpContext.Request.Path
            });
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
            return NotFound(new ProblemDetails
            {
                Title = "User not found",
                Detail = $"User with ID {parameters.Id} does not exist.",
                Status = StatusCodes.Status404NotFound,
                Instance = HttpContext.Request.Path
            });
        }
        _context.Users.Remove(user); //xóa data tìm được khỏi table hiện tại
        await _context.SaveChangesAsync();
        await _messageBus.PublishAsync(new Delete.Messager.Message(parameters.Id, parameters.DeleteNotes)); // bắn qua handler
        return NoContent(); //201
    }
    [HttpPatch]
    public async Task<IActionResult> Patch([FromQuery] Guid id,
                                       [FromBody] JsonPatchDocument<Databases.Journal.Tables.User.Table> patchDoc,
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

        var entity = await _context.Users.FindAsync(id, cancellationToken);
        if (entity == null)
            return NotFound(new ProblemDetails
            {
                Title = "User not found",
                Detail = $"User with ID {id} does not exist.",
                Status = StatusCodes.Status404NotFound,
                Instance = HttpContext.Request.Path
            });

        patchDoc.ApplyTo(entity);

        _context.Users.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
        await _hubContext.Clients.All.SendAsync("week-plan-updated", entity.Id);

        return NoContent();
    }
}