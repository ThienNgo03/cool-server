namespace Journal.Workouts.Delete.Messager;

public record Message(Guid Id, bool IsWeekPlanDelete, bool IsWeekPlanSetDelete);
