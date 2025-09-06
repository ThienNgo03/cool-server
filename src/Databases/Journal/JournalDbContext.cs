namespace Journal.Databases.Journal
{
    public class JournalDbContext : DbContext // database
    {
        public JournalDbContext(DbContextOptions<JournalDbContext> options) : base(options)
        {

        }

        public DbSet<Tables.Journey.Table> Journeys { get; set; } // table 
        public DbSet<Tables.Note.Table> Notes { get; set; }
        public DbSet<Tables.User.Table> Users { get; set; }
        public DbSet<Tables.JourneyUsers.Table> JourneyUsers { get; set; }
        public DbSet<Tables.Gadget.Table> Gadgets { get; set; }
        public DbSet<Tables.JourneyGadgets.Table> JourneyGadgets { get; set; }
        public DbSet<Tables.Exercise.Table> Exercises { get; set; }
        public DbSet<Tables.WorkoutLog.Table> WorkoutLogs { get; set; }
        public DbSet<Tables.Workout.Table> Workouts { get; set; }
        public DbSet<Tables.WeekPlan.Table> WeekPlans { get; set; }
        public DbSet<Tables.MeetUp.Table> MeetUps { get; set; }
        public DbSet<Tables.Competition.Table> Competitions { get; set; }
        public DbSet<Tables.SoloPool.Table> SoloPools { get; set; }
        public DbSet<Tables.TeamPool.Table> TeamPools { get; set; }
        public DbSet<Tables.WorkoutLogSet.Table> WorkoutLogSets { get; set; }
        public DbSet<Tables.WeekPlanSet.Table> WeekPlanSets { get; set; }
        public DbSet<Tables.Muscle.Table> Muscles { get; set; }
        public DbSet<Tables.ExerciseMuscle.Table> ExerciseMuscles { get; set; }
        public DbSet<Tables.Sport.Table> Sport { get; set; }

    }
}

