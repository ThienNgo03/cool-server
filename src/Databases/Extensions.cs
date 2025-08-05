namespace Journal.Databases;

public static class Extensions
{
    public static IServiceCollection AddDatabases(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<JournalDbContext>(x => x.UseSqlServer("Server=localhost;Database=JOURNAL;Trusted_Connection=True;TrustServerCertificate=True;"));

            return services;


    }

}
