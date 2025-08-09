using JasperFx.CodeGeneration.Frames;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Journal.Identity;

[ApiController]
[Route("api/authentication")]
public class Controller:ControllerBase
{
    private readonly ILogger<Controller> _logger;
    private readonly Databases.Identity.IdentityContext _context;
    private readonly UserManager<IdentityUser> _userManager;
    
    private readonly IConfiguration _configuration;
    public Controller(ILogger<Controller> logger, 
        Databases.Identity.IdentityContext context, 
        UserManager<IdentityUser> userManager, 
        IConfiguration configuration)
    {
        _logger = logger;
        _context = context;
        _userManager = userManager;
        _configuration = configuration;
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
            return BadRequest(result.Errors);

        return NoContent();
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

        var token = GenerateToken(payload.AccountEmail);
        var response = new
        {
            Token = token,
            //Expiration = DateTime.UtcNow.AddHours(1),
            //TokenType = "Bearer",
            //Scope = "read write",
        };
        return CreatedAtAction(nameof(Get), response);
    }
    private string GenerateToken(string email)
    {
        var key = Encoding.UTF8.GetBytes(_configuration["JWT:Key"]!);
        var issuer = _configuration["JWT:Issuer"];
        var audience = _configuration["JWT:Audience"];

        var securityKey = new SymmetricSecurityKey(key);
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
