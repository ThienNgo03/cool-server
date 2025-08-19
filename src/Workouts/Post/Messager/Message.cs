namespace Journal.Workouts.Post.Messager
{
    public record Message(Guid Id, ICollection<WeekPlan>? weekPlans);
}
