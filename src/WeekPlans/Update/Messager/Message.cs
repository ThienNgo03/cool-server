namespace Journal.WeekPlans.Update.Messager;

public record Message(Table weekPlan, Guid oldWorkoutId);
