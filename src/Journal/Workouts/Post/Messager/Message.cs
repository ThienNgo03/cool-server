namespace Journal.Workouts.Post.Messager
{
    public record Message(Table workout, ICollection<WeekPlan>? weekPlans);
}
