using JasperFx.CodeGeneration.Frames;
using Microsoft.AspNetCore.Identity;

namespace Journal.Identity;

[ApiController]
[Route("api/identity")]
public class Controller:ControllerBase
{
    private readonly ILogger<Controller> _logger;
    private readonly Databases.Identity.IdentityContext _context;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly Middleware.JWT.Implementation.V1 _jwt;
    public Controller(ILogger<Controller> logger, Databases.Identity.IdentityContext context, 
        UserManager<IdentityUser> userManager, Middleware.JWT.Implementation.V1 jwt)
    {
        _logger = logger;
        _context = context;
        _userManager = userManager;
        _jwt = jwt;
    }
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var users = await _context.Users.ToListAsync();
        if (users == null || users.Count == 0)
        {
            return NotFound("No users found.");
        }
        return Ok(users);   
    }
    [HttpPost]
    [Route("register")]
    public async Task<IActionResult> RegisterAsync([FromBody] Identity.Register.Payload payload)
    {
        var newUser = new IdentityUser
        {
            UserName = payload.AccountName,
            Email = payload.AccountEmail,
            EmailConfirmed = true 
        };
        var result = await _userManager.CreateAsync(newUser, payload.Password);
        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }
        return CreatedAtAction(nameof(Get), new {id=newUser.Id});
    }
    [HttpPost]
    [Route("login")]
    public async Task<IActionResult> LoginAsync([FromBody] Identity.Signin.Payload payload)
    {
        var user = await _userManager.FindByEmailAsync(payload.AccountEmail);
        if (user == null)
        {
            return Unauthorized("Invalid email or password.");
        }
        
        var result = await _userManager.CheckPasswordAsync(user, payload.Password);
        if (!result)
        {
            return Unauthorized("Invalid email or password.");
        }

        var token = _jwt.GenerateToken(payload.AccountEmail);
        var response = new
        {
            Token = token, // Replace with actual token generation logic
            user.Email,
        };
        return CreatedAtAction(nameof(Get), response);
    }
}
