namespace Journal.WeekPlanSets.Patch.Messager;

public record Message(Guid weekPlanSetId, List<(string Path, object? Value)> changes);
