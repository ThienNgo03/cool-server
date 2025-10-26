using Cassandra;
using Journal.Databases.Identity;
using Journal.Databases.Sql;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Journal.Databases.CassandraCql;

namespace Journal.Databases;

public static class Extensions
{
    public static IServiceCollection AddDatabases(this IServiceCollection services, IConfiguration configuration)
    {
        #region Cassandra
        var cassandraDbConfig = configuration.GetSection("CassandraDb").Get<CassandraConfig>();
        if (cassandraDbConfig != null)
        {
            Cluster cluster = Cluster.Builder()
                .AddContactPoint(cassandraDbConfig.ContactPoint)
                .WithPort(cassandraDbConfig.Port)
                .Build();

            Cassandra.ISession session = cluster.Connect(cassandraDbConfig.Keyspace);
            services.AddSingleton<Context>();
            services.AddSingleton(session);
        }
        #endregion
        var journalDbConfig = configuration.GetSection("JournalDb").Get<DbConfig>();
        var identityDbConfig = configuration.GetSection("IdentityDb").Get<DbConfig>();

        if (journalDbConfig == null)
        {
            throw new ArgumentNullException(nameof(journalDbConfig), "JournalDb configuration section is missing or invalid.");
        }
        if (identityDbConfig == null)
        {
            throw new ArgumentNullException(nameof(identityDbConfig), "IdentityDb configuration section is missing or invalid.");
        }
        var journalConnectionString = new ConnectionStringBuilder()
            .WithHost(journalDbConfig.Host)
            .WithPort(journalDbConfig.Port)
            .WithDatabase(journalDbConfig.Database)
            .WithUsername(journalDbConfig.Username)
            .WithPassword(journalDbConfig.Password)
            .WithTrustedConnection()
            .WithTrustServerCertificate()
            .Build();

        var identityConnectionString = new ConnectionStringBuilder()
            .WithHost(identityDbConfig.Host)
            .WithPort(identityDbConfig.Port)
            .WithDatabase(identityDbConfig.Database)
            .WithUsername(identityDbConfig.Username)
            .WithPassword(identityDbConfig.Password)
            .WithTrustedConnection()
            .WithTrustServerCertificate()
            .Build();

        services.AddDbContext<JournalDbContext>(x =>
        {
            x.EnableSensitiveDataLogging();
            x.UseSqlServer(journalConnectionString) // Thêm connection string vào đây
                .UseSeeding((context, _) =>
                {
                    var journalContext = (JournalDbContext)context;
                    App.SeedFactory seedFactory = new();
                    seedFactory.SeedAdmins(journalContext).Wait();
                    seedFactory.SeedExercise(journalContext).Wait();
                    seedFactory.SeedMuscle(journalContext).Wait();
                    seedFactory.SeedExerciseMuscle(journalContext).Wait();
                });
        });

        services.AddDbContext<IdentityContext>(x => 
            x.UseSqlServer(identityConnectionString)
                .UseSeeding((context, _) =>
                {
                    var identityContext = (IdentityContext)context;
                    Identity.SeedFactory seedFactory = new();
                    seedFactory.SeedAdmins(identityContext).Wait();
                }));

        services.AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<IdentityContext>()
                .AddDefaultTokenProviders();

        return services;
    }
}