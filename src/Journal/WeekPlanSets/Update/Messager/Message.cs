namespace Journal.WeekPlanSets.Update.Messager
{
    public record Message(Table weekPlanSet, Guid oldWeekPlanId);
}
