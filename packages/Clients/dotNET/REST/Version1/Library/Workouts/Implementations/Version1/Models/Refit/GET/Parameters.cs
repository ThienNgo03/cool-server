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

    // New include string property for fluent API (e.g., "exercises.muscles,weekPlans")
    public string? Include { get; set; }

    // Legacy boolean properties (deprecated but kept for compatibility)
    public bool IsIncludeWeekPlans { get; set; } = false;

    public bool IsIncludeWeekPlanSets { get; set; } = false;

    public bool IsIncludeExercises { get; set; } = false;

    public bool IsIncludeMuscles { get; set; } = false;
}
