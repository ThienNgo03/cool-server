namespace Journal.Exercises.Patch.Messager;

public record Message(Table exercise, List<(string Path, object? Value)> changes);
