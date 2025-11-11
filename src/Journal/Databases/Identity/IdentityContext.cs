using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Journal.Databases.Identity;

public class IdentityContext: IdentityDbContext
{
    public IdentityContext(DbContextOptions<IdentityContext> options)
        : base(options)
    {
    }
}
