import { portalClient } from "@/api-client";
import { useMutation } from "@tanstack/react-query";
import type { IBaseResponse } from "@/interfaces/response/base";

type CreateExercisePayload = {
    name: string;
    description: string;
    type: string;
};

async function postExercises(payload: CreateExercisePayload): Promise<IBaseResponse> {
    const response = await portalClient.post("/exercises", payload);
    if (response.status !== 201) {
        throw new Error(`Error creating exercise: ${response.statusText}`);
    }
    return response.data;
}

export function useCreateExercise() {
    return useMutation({
        mutationFn: postExercises,
        mutationKey: ["create-exercise"],
    });
}