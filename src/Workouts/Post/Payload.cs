using System.ComponentModel.DataAnnotations;

namespace Journal.Workouts.Post;

public class Payload
{
    [Required]
    public Guid ExerciseId { get; set; }

    [Required]
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
    public int Value { get; set; }
}
