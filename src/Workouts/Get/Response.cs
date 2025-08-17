namespace Journal.Workouts.Get;

public class Response : Databases.Journal.Tables.Workout.Table
{
    public Exercise? Exercise { get; set; }
    public ICollection<WeekPlan>? WeekPlans { get; set; }
}

public class Exercise : Databases.Journal.Tables.Exercise.Table
{
}

public class WeekPlan : Databases.Journal.Tables.WeekPlan.Table
{
    public ICollection<WeekPlanSet>? WeekPlanSets { get; set; }
}

public class WeekPlanSet : Databases.Journal.Tables.WeekPlanSet.Table
{
}
