namespace Journal.Workouts.Patch.Messager;

public record Message(Table entity, List<(string Path, object? Value)> changes);
