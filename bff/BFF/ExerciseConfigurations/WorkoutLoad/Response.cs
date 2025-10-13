namespace BFF.ExerciseConfigurations.WorkoutLoad;

public class Response : Databases.App.Tables.Workout.Table
{
    public Exercise? Exercise { get; set; }
    public ICollection<WeekPlan>? WeekPlans { get; set; }
}

public class Exercise : Databases.App.Tables.Exercise.Table
{
    public ICollection<Muscle>? Muscles { get; set; }
}

public class Muscle : Databases.App.Tables.Muscle.Table
{
}

public class WeekPlan : Databases.App.Tables.WeekPlan.Table
{
    public ICollection<WeekPlanSet>? WeekPlanSets { get; set; }
}

public class WeekPlanSet : Databases.App.Tables.WeekPlanSet.Table
{
}
