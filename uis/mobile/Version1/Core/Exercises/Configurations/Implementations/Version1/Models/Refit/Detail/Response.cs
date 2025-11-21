namespace Core.Exercises.Configurations.Implementations.Version1.Models.Refit.Detail;

public class Response
{
    public Guid WorkoutId { get; set; }
    public Guid UserId { get; set; }
    public ICollection<WeekPlan>? WeekPlans { get; set; }
}


public class WeekPlan
{
    public string? DateOfWeek { get; set; }
    public TimeSpan Time { get; set; }
    public ICollection<WeekPlanSet>? WeekPlanSets { get; set; }
}

public class WeekPlanSet
{
    public Guid Id { get; set; }
    public int Value { get; set; }
}
