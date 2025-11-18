using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Caching.Memory;

public class HmacAuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConfiguration _configuration;
    private readonly IMemoryCache _cache;

    public HmacAuthenticationMiddleware(RequestDelegate next, IConfiguration configuration, IMemoryCache cache)
    {
        _next = next;
        _configuration = configuration;
        _cache = cache;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? "";

        if (path.StartsWith("/api/authentication", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        // ✅ Nếu JWT đã xác thực → cho qua, không xử lý HMAC
        if (context.User?.Identity?.IsAuthenticated == true)
        {
            await _next(context);
            return;
        }

        var headers = context.Request.Headers;
        var timestamp = headers["X-Timestamp"].FirstOrDefault();
        var machineHash = headers["X-Machine-Hash"].FirstOrDefault();
        var nonce = headers["X-Nonce"].FirstOrDefault();
        var secretKey = _configuration["MachineAuth:SecretKey"];

        // ✅ Nếu không có HMAC → cho qua để JWT xử lý
        if (string.IsNullOrEmpty(timestamp) || string.IsNullOrEmpty(machineHash) || string.IsNullOrEmpty(nonce))
        {
            await _next(context);
            return;
        }

        // ✅ Nếu nonce đã dùng → chặn
        var nonceKey = $"hmac-nonce:{nonce}";
        if (_cache.TryGetValue(nonceKey, out _))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Replay attack detected.");
            return;
        }

        _cache.Set(nonceKey, true, TimeSpan.FromMinutes(5));

        var message = secretKey + timestamp + nonce;
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey));
        var computedHash = BitConverter.ToString(hmac.ComputeHash(Encoding.UTF8.GetBytes(message))).Replace("-", "").ToLower();

        if (computedHash != machineHash.ToLower())
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Invalid HMAC.");
            return;
        }

        // ✅ Chỉ gán User nếu chưa có JWT
        if (context.User?.Identity?.IsAuthenticated != true)
        {
            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, "BFF"),
            new Claim(ClaimTypes.Role, "Machine"),
            new Claim("AuthType", "HMAC")
        };
            var identity = new ClaimsIdentity(claims, "HMAC");
            context.User = new ClaimsPrincipal(identity);
        }

        await _next(context);
    }
}