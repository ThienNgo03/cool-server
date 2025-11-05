namespace Journal.Muscles.Update.Messager;

public record Message(Guid muscleId, Table updatedMuscle);