using Microsoft.EntityFrameworkCore;

namespace BFF.Databases.App;

public class JournalDbContext(DbContextOptions<JournalDbContext> options) : DbContext(options)
{
    public DbSet<Tables.User.Table> Users { get; set; }
    public DbSet<Tables.Exercise.Table> Exercises { get; set; }
    public DbSet<Tables.Workout.Table> Workouts { get; set; }
    public DbSet<Tables.WeekPlan.Table> WeekPlans { get; set; }
    public DbSet<Tables.WeekPlanSet.Table> WeekPlanSets { get; set; }
    public DbSet<Tables.ExerciseMuscle.Table> ExerciseMuscles { get; set; }
    public DbSet<Tables.Muscle.Table> Muscles { get; set; }
    public DbSet<Tables.WorkoutLog.Table> WorkoutLogs { get; set; }
}
