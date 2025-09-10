using Grpc.Core;
using Journal.Beta.Authentication.Login;
using Journal.Databases.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Journal.Beta.Authentication.Login;

public class Service:Method.MethodBase
{
    private readonly IdentityContext _context;
    private readonly ILogger<Service> _logger;
    private readonly IConfiguration _configuration;
    private readonly UserManager<IdentityUser> _userManager;
    public Service(ILogger<Service> logger, 
        UserManager<IdentityUser> userManager,
        IConfiguration configuration,
        IdentityContext context)
    {
        _logger = logger;
        _userManager = userManager;
        _configuration = configuration;
        _context = context;
    }

    public override async Task<Result> Login(Payload payload, ServerCallContext context)
    {
        var user = await _userManager.FindByEmailAsync(payload.Email) ?? throw new RpcException(new Status(StatusCode.InvalidArgument, "Email does not exists."));
        var result = await _userManager.CheckPasswordAsync(user, payload.Password);
        if (!result)
        {
            throw new RpcException(new Status(StatusCode.Unauthenticated, "Invalid email or password."));
        }

        var token = GenerateToken(payload.Email);
        Result response = new()
        {
            Token=token,
        };
        return response;
    }
    private string GenerateToken(string email)
    {
        var key = Encoding.UTF8.GetBytes(_configuration["JWT:Key"]!);
        var issuer = _configuration["JWT:Issuer"];
        var audience = _configuration["JWT:Audience"];

        var user = _context.Users.FirstOrDefault(u => u.Email == email);
        if (user == null)
            throw new InvalidOperationException("User not found");

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var securityKey = new SymmetricSecurityKey(key);
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
