using Cassandra;

namespace BFF.Database;

public static class Extension
{
    public static IServiceCollection AddDatabase(this IServiceCollection services)
    {
        Cluster cluster = Cluster.Builder()
                .AddContactPoint("localhost")
                .WithPort(int.Parse("9042"))
                .Build();

        Cassandra.ISession session = cluster.Connect("bff");
        services.AddSingleton<Database.Messages.Context>();
        services.AddSingleton<Cassandra.ISession>(session);

        return services;

    }
}
