namespace Library.Workouts.All;

public class Parameters
{
    public Guid? Id { get; set; }

    public Guid? ExerciseId { get; set; }

    public Guid? UserId { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? LastUpdated { get; set; }

    public int? PageSize { get; set; }
    public int? PageIndex { get; set; }
    
    // New include string property (future use)
    public string? Include { get; set; }
    
    // Legacy boolean properties (will be deprecated)
    public bool IsIncludeWeekPlans { get; set; } = false;
    public bool IsIncludeWeekPlanSets { get; set; } = false;
    public bool IsIncludeExercises { get; set; } = false;
    public bool IsIncludeMuscles { get; set; } = false;
}
