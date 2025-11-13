import { portalClient } from "@/api-client";
import { useMutation } from "@tanstack/react-query";

type DeleteExerciseQuery = {
    exerciseId: string;
    isDeleteWorkouts?: boolean;
};

async function deleteExercises({ exerciseId, isDeleteWorkouts = false }: DeleteExerciseQuery): Promise<void> {
    const response = await portalClient.delete("/exercises", { params: { id: exerciseId, isDeleteWorkouts } });
    if (response.status !== 204) {
        throw new Error(`Error deleting exercise: ${response.statusText}`);
    }
    return response.data;
}

export function useDeleteExercise() {
    return useMutation({
        mutationFn: deleteExercises,
        mutationKey: ["delete-exercise"],
    });
}