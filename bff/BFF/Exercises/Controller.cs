using BFF.Databases.App;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BFF.Exercises;

[Route("api/exercises")]
[ApiController]
public class Controller : ControllerBase
{
    private readonly JournalDbContext _context;
    private readonly IMapper _mapper;
    private readonly Library.Exercises.Interface _exercises;
    private readonly Library.Muscles.Interface _muscles;
    public Controller(JournalDbContext context, IMapper mapper, Library.Exercises.Interface exercises, Library.Muscles.Interface muscles)
    {
        _context = context;
        _mapper = mapper;
        _exercises = exercises;
        _muscles = muscles;
    }

    [HttpGet("all")]
    public async Task<IActionResult> All([FromQuery] Library.Exercises.GET.Parameters parameters, CancellationToken cancellationToken = default!)
    {
        var exercises = await _exercises.GetAsync(parameters);
        var items = exercises.Items?.Select(e => new All.Item
        {
            Id = e.Id,
            Title = e.Name,
            Description = e.Description
        }).ToList();
        if (items is null || !items.Any())
        {
            return Ok(new List<All.Item>());
        }

        await _mapper.All.SetSubTitle(items);
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
        var muscles = await _muscles.GetAsync(new Library.Muscles.GET.Parameters());
        if (muscles.Items is null || !muscles.Items.Any())
        {
            return Ok(new List<string>());
        }
        var response = muscles.Items?.Select(m => m.Name).ToList();
        //Select(m => m.Name).ToListAsync();
        //return list of muscle names as categories
        return Ok(response);
    }
}
