using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BFF.Databases.Identity;

public class IdentityContext(DbContextOptions<IdentityContext> options) : IdentityDbContext(options)
{

}
