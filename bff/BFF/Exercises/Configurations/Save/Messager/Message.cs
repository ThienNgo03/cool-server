using BFF.Exercises.Configurations.Save;

namespace BFF.Exercises.Configurations.Save.Messager
{
    public record Message(Guid Id, 
        ICollection<WeekPlan>? weekPlans,
        Guid ExerciseId,
        Guid UserId,
        ICollection<Guid> OldWorkoutId);
}
