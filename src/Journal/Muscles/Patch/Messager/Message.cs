namespace Journal.Muscles.Patch.Messager;

public record Message(Guid muscleId, List<(string Path, object? Value)> changes);