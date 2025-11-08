using BFF.Databases.App;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using static Azure.Core.HttpHeader;

namespace BFF.Exercises;

[Route("api/exercises")]
[ApiController]
public class Controller : ControllerBase
{
    private readonly JournalDbContext _context;
    private readonly IMapper _mapper;
    public Controller(JournalDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    [HttpGet("all")]
    public async Task<IActionResult> All([FromQuery] All.Parameters parameters, CancellationToken cancellationToken = default!)
    {
        var exercises = await _context.Exercises.ToListAsync(cancellationToken);
        var items = exercises.Select(e => new All.Item
        {
            Id = e.Id,
            Title = e.Name,
            Description = e.Description
        }).ToList();

        _mapper.All.SetSubTitle(items);
        _mapper.All.AttachImageUrls(items);
        _mapper.All.SetBadge(items);
        _mapper.All.SetPercentageCompletion(items);
        _mapper.All.SetPercentageCompletionString(items);
        _mapper.All.SetBadgeTextColor(items);
        _mapper.All.SetBadgeBackgroundColor(items);
        return Ok(items);
    }

    [HttpGet("categories")]
    public async Task<IActionResult> Categories()
    {
        //return a list of muscle names from the muscles table
        var response = await _context.Muscles.Select(m => m.Name).ToListAsync();
        //return list of muscle names as categories
        return Ok(response);
    }
}
