namespace Journal.Databases.App;

public class JournalDbContext : DbContext 
{

    public JournalDbContext(
        DbContextOptions<JournalDbContext> options) : base(options)
    {
    }
    public DbSet<Competitions.Table> Competitions { get; set; }
    public DbSet<ExerciseMuscles.Table> ExerciseMuscles { get; set; }
    public DbSet<Exercises.Table> Exercises { get; set; }
    public DbSet<Gadgets.Table> Gadgets { get; set; }
    public DbSet<Muscles.Table> Muscles { get; set; }
    public DbSet<Journeys.Table> Journeys { get; set; }
    public DbSet<MeetUps.Table> MeetUps { get; set; }
    public DbSet<Notes.Table> Notes { get; set; }
    public DbSet<Profiles.Table> Profiles { get; set; }
    public DbSet<JourneyGadgets.Table> JourneyGadgets { get; set; }
    public DbSet<JourneyUsers.Table> JourneyUsers { get; set; }
    public DbSet<WorkoutLogs.Table> WorkoutLogs { get; set; }
    public DbSet<Workouts.Table> Workouts { get; set; }
    public DbSet<WeekPlans.Table> WeekPlans { get; set; }
    public DbSet<SoloPools.Table> SoloPools { get; set; }
    public DbSet<Sports.Table> Sports { get; set; }
    public DbSet<TeamPools.Table> TeamPools { get; set; }
    public DbSet<WorkoutLogSets.Table> WorkoutLogSets { get; set; }
    public DbSet<WeekPlanSets.Table> WeekPlanSets { get; set; }
}



