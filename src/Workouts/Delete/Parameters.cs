namespace Journal.Workouts.Delete;

public class Parameters
{
    public Guid Id { get; set; }

    public bool IsWeekPlanDelete { get; set; } = false;

    public bool IsWeekPlanSetDelete { get; set; } = false;

    public bool IsDeleteAll { get; set; }
}
