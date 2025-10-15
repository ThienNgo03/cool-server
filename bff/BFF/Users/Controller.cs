using BFF.Databases.App;
using BFF.Models.PaginationResults;
using BFF.Users.All;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Wolverine;

namespace BFF.Users;

[ApiController]
[Route("api/users")]
//[Authorize]
public class Controller : ControllerBase
{
    private readonly IMessageBus _messageBus;

    private readonly ILogger<Controller> _logger;

    private readonly JournalDbContext _context;
    private readonly IHubContext<Hub> _hubContext;
    private readonly IMapper _mapper;
    public Controller(ILogger<Controller> logger, 
        JournalDbContext context, 
        IMessageBus messageBus, 
        IHubContext<Hub> hubContext,
        IMapper mapper)
    {
        _logger = logger;
        _context = context;
        _messageBus = messageBus;
        _hubContext = hubContext;
        _mapper = mapper;
    }

    [HttpGet("all")]
    [AllowAnonymous]
    public async Task<IActionResult> All([FromQuery] All.Parameters parameters, CancellationToken cancellationToken = default!)
    {
        var query = _context.Users.AsQueryable();

        if (parameters.UserId is not null)
            query = query.Where(u => u.Id == parameters.UserId.Value);

        if (!string.IsNullOrEmpty(parameters.UserName))
            query = query.Where(x => x.Name!.Contains(parameters.UserName));

        var result = await query.AsNoTracking().ToListAsync(cancellationToken);
        var response = result.Select(u => new All.Response
        {
            UserId = u.Id,
            Title = u.Name,
            SubTitle = u.Email,
            ImageUrl = _mapper.All.SetImageUrl(),
            Time = _mapper.All.SetTime(),
            Status = _mapper.All.SetStatus()
        }).ToList();
        return Ok(response);
    }
}