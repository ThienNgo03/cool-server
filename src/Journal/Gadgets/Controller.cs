namespace Journal.Gadgets;

[ApiController]
[Route("api/gadgets")]

public class Controller:ControllerBase
{
    private readonly ILogger<Controller> _logger;
    private readonly JournalDbContext _context;
    public Controller(ILogger<Controller> logger, JournalDbContext context)
    {
        _logger = logger;
        _context = context;
    }
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] Get.Parameters parameters)
    {
        var query =_context.Gadgets.AsQueryable();
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
        if (!string.IsNullOrEmpty(parameters.Name))
            query = query.Where(g => g.Name.Contains(parameters.Name, StringComparison.OrdinalIgnoreCase));
        if(!string.IsNullOrEmpty(parameters.Brand))
            query = query.Where(g => g.Brand.Contains(parameters.Brand, StringComparison.OrdinalIgnoreCase));
        if(!string.IsNullOrEmpty(parameters.Description))
            query = query.Where(g => g.Description.Contains(parameters.Description, StringComparison.OrdinalIgnoreCase));
        if (parameters.Date.HasValue)
            query = query.Where(x => x.Date == parameters.Date);
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
        if (parameters.PageSize.HasValue && parameters.PageIndex.HasValue && parameters.PageSize > 0 && parameters.PageIndex >= 0)
            query = query.Skip(parameters.PageIndex.Value * parameters.PageSize.Value).Take(parameters.PageSize.Value);

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
    public async Task<IActionResult> Create([FromBody] Post.Payload payload)
    {
        var gadget = new Table
        {
            Id = Guid.NewGuid(),
            Name = payload.Name,
            Description = payload.Description,
            Brand = payload.Brand,
            Date = payload.Date,
        };
        _context.Gadgets.Add(gadget);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), gadget.Id);
    }
    [HttpPut]
    public async Task<IActionResult> Update([FromBody] Update.Payload payload)
    {
        var gadget = await _context.Gadgets.FindAsync(payload.Id);
        if (gadget == null)
        {
            return NotFound();
        }
        gadget.Name = payload.Name;
        gadget.Description = payload.Description;
        gadget.Brand = payload.Brand;
        gadget.Date = payload.Date;
        _context.Gadgets.Update(gadget);
        await _context.SaveChangesAsync();
        return NoContent();
    }
    [HttpDelete]
    public async Task<IActionResult> Delete(Delete.Paramters paramters)
    {
        var gadget = await _context.Gadgets.FindAsync(paramters.Id);
        if (gadget == null)
        {
            return NotFound();
        }
        _context.Gadgets.Remove(gadget);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
