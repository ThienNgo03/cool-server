namespace BFF.ExerciseConfigurations.Save.Messager
{
    public record Message(Guid Id, 
        ICollection<WeekPlan>? weekPlans,
        Guid ExerciseId,
        Guid UserId);
}
