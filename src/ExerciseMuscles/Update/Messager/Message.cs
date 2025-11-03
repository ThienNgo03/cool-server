using Spectre.Console.Rendering;

namespace Journal.ExerciseMuscles.Update.Messager;
public record Message(
    Table exerciseMuscles,
    Guid oldExerciseId,
    Guid oldMuscleId,
    Guid newExerciseId,
    Guid newMuscleId
);
