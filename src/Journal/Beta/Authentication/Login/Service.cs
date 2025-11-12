using Grpc.Core;
using Journal.Beta.Authentication.Login;
using Journal.Databases.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;

namespace Journal.Beta.Authentication.Login;

public class Service : LoginMethod.LoginMethodBase
{
    private readonly IdentityContext _context;
    private readonly ILogger<Service> _logger;
    private readonly IConfiguration _configuration;
    private readonly UserManager<IdentityUser> _userManager;
    private static readonly Regex EmailRegex = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);
    private static readonly Regex VietnamesePhoneRegex = new(@"^(?:\+84|0)(?:3[2-9]|5[6|8|9]|7[0|6-9]|8[1-5]|9[0-9])[0-9]{7}$", RegexOptions.Compiled);
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
        IdentityUser user = new();
        if (EmailRegex.IsMatch(payload.Account))
        {
            user = await _context.Users.FirstOrDefaultAsync(u => u.Email == payload.Account) ?? throw new RpcException(new Status(StatusCode.Unavailable, "Email does not exist"));
        }
        if (VietnamesePhoneRegex.IsMatch(payload.Account))
        {
            user = await _context.Users.FirstOrDefaultAsync(u => u.PhoneNumber == payload.Account) ?? throw new RpcException(new Status(StatusCode.Unavailable, "Phone number does not exist"));
        }

        var result = await _userManager.CheckPasswordAsync(user, payload.Password);
        if (!result)
        {
            throw new RpcException(new Status(StatusCode.Unauthenticated, "Invalid email or password."));
        }

        var token = GenerateToken(user.Email!);
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
