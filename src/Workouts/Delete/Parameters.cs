namespace Journal.Workouts.Delete;

public class Parameters
{
    public Guid Id { get; set; }

    public bool IsWeekPlanDelete { get; set; }

    public bool IsWeekPlanSetDelete { get; set; }

    public bool IsDeleteAll { get; set; }
}
