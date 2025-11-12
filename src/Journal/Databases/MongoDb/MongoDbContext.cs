namespace Journal.Databases.MongoDb;

public class MongoDbContext : DbContext // database
{

    public MongoDbContext(DbContextOptions<MongoDbContext> options) : base(options)
    {
        Database.AutoTransactionBehavior = AutoTransactionBehavior.Never;
    }
    public DbSet<Collections.Workout.Collection> Workouts { get; init; }

    public DbSet<Collections.Exercise.Collection> Exercises { get; init; }
}
