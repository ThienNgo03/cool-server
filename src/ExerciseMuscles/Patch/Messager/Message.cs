namespace Journal.ExerciseMuscles.Patch.Messager;

public record Message(ExerciseMuscles.Table entity, List<(string Path, object? Value)> changes);