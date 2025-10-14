namespace BFF.ExerciseConfigurations.Detail;

public class Response 
{
    public Guid WorkoutId { get; set; }
    public Guid UserId { get; set; }
    public Exercise? Exercise { get; set; }
    public string? PercentageCompletion { get; set; }
    public string? Difficulty { get; set; }
    public ICollection<WeekPlan>? WeekPlans { get; set; }
}

public class Exercise 
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public ICollection<Muscle>? Muscles { get; set; }
}

public class Muscle 
{
    public string? Name { get; set; }
}

public class WeekPlan 
{
    public TimeSpan Time { get; set; }
    public string? DateOfWeek { get; set; }
    public ICollection<WeekPlanSet>? WeekPlanSets { get; set; }
}

public class WeekPlanSet 
{
    public int Value { get; set; }
}
