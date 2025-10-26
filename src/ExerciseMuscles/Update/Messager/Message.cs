using Spectre.Console.Rendering;

namespace Journal.ExerciseMuscles.Update.Messager;

public record Message(Guid id, Guid exerciseId);
