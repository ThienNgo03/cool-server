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
        if(parameters.Id.HasValue)
            query = query.Where(g => g.Id == parameters.Id.Value);
        if(!string.IsNullOrEmpty(parameters.Name))
            query = query.Where(g => g.Name.Contains(parameters.Name, StringComparison.OrdinalIgnoreCase));
        if(!string.IsNullOrEmpty(parameters.Brand))
            query = query.Where(g => g.Brand.Contains(parameters.Brand, StringComparison.OrdinalIgnoreCase));
        if(!string.IsNullOrEmpty(parameters.Description))
            query = query.Where(g => g.Description.Contains(parameters.Description, StringComparison.OrdinalIgnoreCase));
        if (parameters.Date.HasValue)
            query = query.Where(x => x.Date == parameters.Date);
        if (parameters.PageSize.HasValue && parameters.PageIndex.HasValue && parameters.PageSize > 0 && parameters.PageIndex >= 0)
            query = query.Skip(parameters.PageIndex.Value * parameters.PageSize.Value).Take(parameters.PageSize.Value);

        var result = await query.AsNoTracking().ToListAsync();
        return Ok(result);
    }
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Post.Payload payload)
    {
        var gadget = new Databases.Journal.Tables.Gadget.Table
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
