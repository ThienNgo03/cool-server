using ExcelDataReader;
using Journal.Databases.Identity;
using Microsoft.AspNetCore.Identity;
using System.Data;

namespace Journal.Databases;

public static class Extensions
{
    public static IServiceCollection AddDatabases(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<JournalDbContext>(x =>
        {
                x.EnableSensitiveDataLogging();
                x.UseSqlServer("Server=localhost;Database=JournalTest10;Trusted_Connection=True;TrustServerCertificate=True;")
                    .UseSeeding((context, _) =>
                    {
                        var journalContext = (JournalDbContext)context;
                        SeedFactory seedFactory = new SeedFactory();
                        seedFactory.SeedExercise(journalContext).Wait();
                        seedFactory.SeedMuscle(journalContext).Wait();
                        seedFactory.SeedExerciseMuscle(journalContext).Wait();
                    });
                });
        services.AddDbContext<IdentityContext>(x => x.UseSqlServer("Server=localhost;Database=Identity;Trusted_Connection=True;TrustServerCertificate=True;"));

        services.AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<IdentityContext>()
                .AddDefaultTokenProviders();

        return services;


    }

}
