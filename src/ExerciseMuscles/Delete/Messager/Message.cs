namespace Journal.ExerciseMuscles.Delete.Messager;

public record Message(Guid id, Guid exerciseId, Guid muscleId);
