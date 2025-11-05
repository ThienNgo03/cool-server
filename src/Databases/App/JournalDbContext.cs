using Journal.ExerciseMuscles.Tables.App;

namespace Journal.Databases.App;

public class JournalDbContext : DbContext // database
{

    public JournalDbContext(
        DbContextOptions<JournalDbContext> options) : base(options)
    {
    }
    public DbSet<Exercises.Table> Exercises { get; set; }
    public DbSet<Muscles.Table> Muscles { get; set; }
    public DbSet<Tables.Journey.Table> Journeys { get; set; } // table 
    public DbSet<Tables.Note.Table> Notes { get; set; }
    public DbSet<Users.Table> Users { get; set; }
    public DbSet<Tables.JourneyUsers.Table> JourneyUsers { get; set; }
    public DbSet<Tables.Gadget.Table> Gadgets { get; set; }
    public DbSet<Tables.JourneyGadgets.Table> JourneyGadgets { get; set; }
    public DbSet<WorkoutLogs.Table> WorkoutLogs { get; set; }
    public DbSet<Workouts.Table> Workouts { get; set; }
    public DbSet<WeekPlans.Table> WeekPlans { get; set; }
    public DbSet<Tables.MeetUp.Table> MeetUps { get; set; }
    public DbSet<Tables.Competition.Table> Competitions { get; set; }
    public DbSet<Tables.SoloPool.Table> SoloPools { get; set; }
    public DbSet<Tables.TeamPool.Table> TeamPools { get; set; }
    public DbSet<WorkoutLogSets.Table> WorkoutLogSets { get; set; }
    public DbSet<WeekPlanSets.Table> WeekPlanSets { get; set; }
    public DbSet<Table> ExerciseMuscles { get; set; }
    public DbSet<Tables.Sport.Table> Sport { get; set; }
}



