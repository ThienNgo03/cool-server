namespace Journal.WeekPlans.Delete.Messager
{
    public record Message(Delete.Parameters parameters, Guid workoutId);
}
