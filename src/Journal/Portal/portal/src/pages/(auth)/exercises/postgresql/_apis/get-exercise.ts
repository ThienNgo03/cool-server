import { useQuery } from "@tanstack/react-query";
import { useEffect } from "react";
import { toast } from "sonner";
import { portalClient } from "@/api-client";
import type { IExercise } from "@/interfaces/models/exercise";
import type { IBaseRequest } from "@/interfaces/request/base";
import type { IBaseGetResponse } from "@/interfaces/response/base-get";
import { getErrorMessage } from "@/lib/getErrorMessage";

export interface GetExerciseRequest extends IBaseRequest<IExercise> {
    name?: string;
    description?: string;
    type?: string;
    musclesSortBy?: string;
    musclesSortOrder?: 'asc' | 'desc';
}

async function getExercises(params: GetExerciseRequest): Promise<IBaseGetResponse<IExercise>> {
    const response = await portalClient.get("/exercises", { params });
    if (response.status !== 200) {
        throw new Error(`Error fetching exercises: ${response.statusText}`);
    }
    return response.data;
}

export function useGetExercises(params: GetExerciseRequest) {
    const { data, refetch, error, isError, isLoading, isFetching } = useQuery({
        queryKey: ["exercises", params],
        queryFn: () => getExercises(params),
    })

    useEffect(() => {
        if (isError) {
            toast.error(`Error fetching exercises`, {
                description: getErrorMessage(error),
            });
        }
    }, [isError, error]);

    return { data, refetch, isLoading: isLoading || isFetching };
}