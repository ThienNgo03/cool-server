namespace Journal.Workouts.Get;

public class Response : Databases.App.Tables.Workout.Table
{
    public Exercise? Exercise { get; set; }
    public ICollection<WeekPlan>? WeekPlans { get; set; }
}

public class Exercise : Exercises.Table
{
    public ICollection<Muscle>? Muscles { get; set; }
}

public class Muscle : Muscles.Table
{
}

public class WeekPlan : Databases.App.Tables.WeekPlan.Table
{
    public ICollection<WeekPlanSet>? WeekPlanSets { get; set; }
}

public class WeekPlanSet : Databases.App.Tables.WeekPlanSet.Table
{
}
