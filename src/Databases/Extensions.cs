using Journal.Databases.Identity;
using Microsoft.AspNetCore.Identity;

namespace Journal.Databases;

public static class Extensions
{
    public static IServiceCollection AddDatabases(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<JournalDbContext>(x => x.UseSqlServer("Server=localhost,1433;Database=Journal;User Id=sa;Password=SqlServer2022!;TrustServerCertificate=true;"));
        services.AddDbContext<IdentityContext>(x => x.UseSqlServer("Server=localhost,1433;Database=IdentityDb;User Id=sa;Password=SqlServer2022!;TrustServerCertificate=true;"));
        services.AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<IdentityContext>()
                .AddDefaultTokenProviders();

        return services;


    }

}
