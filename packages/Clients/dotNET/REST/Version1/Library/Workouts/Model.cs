namespace Library.Workouts;

public class Model
{
    public Guid Id { get; set; }

    public Guid ExerciseId { get; set; }

    public Guid UserId { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime LastUpdated { get; set; }

    public Exercise? Exercise { get; set; }

    public ICollection<WeekPlan>? WeekPlans { get; set; }
}

public class WeekPlan
{
    public Guid Id { get; set; }

    public Guid WorkoutId { get; set; }

    public string DateOfWeek { get; set; }

    public TimeSpan Time { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? LastUpdated { get; set; }

    public ICollection<WeekPlanSet>? WeekPlanSets { get; set; }
}

public class WeekPlanSet
{
    public Guid Id { get; set; }
    public DateTime CreatedDate { get; set; }
    public Guid InsertedBy { get; set; }
    public DateTime? LastUpdated { get; set; }
    public Guid UpdatedBy { get; set; }
    public Guid WeekPlanId { get; set; }
    public int Value { get; set; }
}

public class Exercise
{
    public Guid Id { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public string Type { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime LastUpdated { get; set; }

    public ICollection<ExerciseMuscle>? ExerciseMuscles { get; set; }

}

public class ExerciseMuscle
{
    public Guid Id { get; set; }

    public string Name { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime LastUpdated { get; set; }
}