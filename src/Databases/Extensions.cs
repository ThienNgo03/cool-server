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
                x.UseSqlServer("Server=localhost;Database=JournalTest2;Trusted_Connection=True;TrustServerCertificate=True;")
                    .UseSeeding((context, _) =>
                    {
                        var journalContext = (JournalDbContext)context;
                        Journal.SeedFactory seedFactory = new ();
                        seedFactory.SeedAdmins(journalContext).Wait();
                        seedFactory.SeedExercise(journalContext).Wait();
                        seedFactory.SeedMuscle(journalContext).Wait();
                        seedFactory.SeedExerciseMuscle(journalContext).Wait();
                    });
                });
        services.AddDbContext<IdentityContext>(x => x.UseSqlServer("Server=localhost;Database=Identity2;Trusted_Connection=True;TrustServerCertificate=True;")
        .UseSeeding((context, _) =>
        {
            var identityContext = (IdentityContext)context;
            Identity.SeedFactory seedFactory = new ();
            seedFactory.SeedAdmins(identityContext).Wait();
        }));

        services.AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<IdentityContext>()
                .AddDefaultTokenProviders();

        return services;


    }

}
