namespace Journal.WeekPlans.Patch.Messager;

public record Message(Guid weekPlanId, List<(string Path, object? Value)> changes);
