using Cassandra;
using Journal.Databases.Identity;
using Journal.Databases.MongoDb;
using Journal.Databases.Sql;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
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
        var journalDbConfig = configuration.GetSection("JournalDb").Get<Sql.DbConfig>();
        var identityDbConfig = configuration.GetSection("IdentityDb").Get<Sql.DbConfig>();

        if (journalDbConfig == null)
        {
            throw new ArgumentNullException(nameof(journalDbConfig), "JournalDb configuration section is missing or invalid.");
        }
        if (identityDbConfig == null)
        {
            throw new ArgumentNullException(nameof(identityDbConfig), "IdentityDb configuration section is missing or invalid.");
        }
        var journalConnectionString = new Sql.ConnectionStringBuilder()
            .WithHost(journalDbConfig.Host)
            .WithPort(journalDbConfig.Port)
            .WithDatabase(journalDbConfig.Database)
            .WithUsername(journalDbConfig.Username)
            .WithPassword(journalDbConfig.Password)
            .WithTrustedConnection()
            .WithTrustServerCertificate()
            .Build();

        var identityConnectionString = new Sql.ConnectionStringBuilder()
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

        var mongoDbConfig = configuration.GetSection("MongoDb").Get<MongoDb.DbConfig>();
        if (mongoDbConfig == null)
        {
            throw new ArgumentNullException(nameof(mongoDbConfig), "MongoDb configuration section is missing or invalid.");
        }

        var mongoConnectionStringBuilder = new MongoDb.ConnectionStringBuilder()
            .WithHost(mongoDbConfig.Host)
            .WithPort(mongoDbConfig.Port)
            .WithDatabase(mongoDbConfig.Database)
            .WithUsername(mongoDbConfig.Username)
            .WithPassword(mongoDbConfig.Password)
            .WithAuthDatabase(mongoDbConfig.AuthDatabase);

        var mongoConnectionString = mongoConnectionStringBuilder.Build();
        var databaseName = mongoConnectionStringBuilder.GetDatabaseName();

        var client = new MongoClient(mongoConnectionString);
        var database = client.GetDatabase(databaseName);

        services.AddDbContext<MongoDbContext>(options =>
            options.UseMongoDB(client, database.DatabaseNamespace.DatabaseName)
        );

        return services;
    }
}