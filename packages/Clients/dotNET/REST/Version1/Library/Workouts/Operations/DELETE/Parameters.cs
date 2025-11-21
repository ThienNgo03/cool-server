namespace Library.Workouts.DELETE;

public class Parameters
{
    public Guid Id { get; set; }

    public bool IsWeekPlanDelete { get; set; } = false;

    public bool IsWeekPlanSetDelete { get; set; } = false;
}
