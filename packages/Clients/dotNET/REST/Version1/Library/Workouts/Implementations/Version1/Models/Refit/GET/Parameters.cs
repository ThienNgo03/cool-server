namespace Library.Workouts.Implementations.Version1.Models.Refit.GET;

public class Parameters
{
    public Guid? Id { get; set; }

    public Guid? ExerciseId { get; set; }

    public Guid? UserId { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? LastUpdated { get; set; }

    public int? PageSize { get; set; }

    public int? PageIndex { get; set; }

    public bool IsIncludeWeekPlans { get; set; } = false;

    public bool IsIncludeWeekPlanSets { get; set; } = false;

    public bool IsIncludeExercises { get; set; } = false;

    public bool IsIncludeExerciseMuscles { get; set; } = false;
}
