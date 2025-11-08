namespace Journal.ExerciseMuscles.Patch.Messager;

public record Message(Table entity, List<(string Path, object? Value)> changes, Guid oldExerciseId, Guid oldMuscleId);