namespace Library.Workouts.Implementations.Version1.Models.Refit.POST;

public class Payload
{
    public Guid ExerciseId { get; set; }

    public Guid UserId { get; set; }

    public ICollection<WeekPlan>? WeekPlans { get; set; }
}

public class WeekPlan
{
    public string DateOfWeek { get; set; }

    public TimeSpan Time { get; set; }
    public ICollection<WeekPlanSet>? WeekPlanSets { get; set; }
}

public class WeekPlanSet
{
    public int value { get; set; }
}