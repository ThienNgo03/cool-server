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
        var response = exercises.Select(e => new All.Response
        {
            Id = e.Id,
            Title = e.Name,
            Description = e.Description
        }).ToList();

        _mapper.All.SetSubTitle(response);
        _mapper.All.AttachImageUrls(response);
        _mapper.All.SetBadge(response);
        _mapper.All.SetPercentageCompletion(response);
        _mapper.All.SetPercentageCompletionString(response);
        _mapper.All.SetBadgeTextColor(response);
        _mapper.All.SetBadgeBackgroundColor(response);
        return Ok(response);
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
