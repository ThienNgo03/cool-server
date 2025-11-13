import { useSearchParams } from "react-router-dom";
import { useGetExercises } from "../_apis/get-exercise";

export function useListExercise() {
    const [searchParams] = useSearchParams();
    const request = parseParams(searchParams);
    const { data, refetch, isLoading } = useGetExercises(request);

    return { data, refetch, isLoading };
}

function parseParams(params: URLSearchParams) {
    const pageIndex = params.get("pageIndex");
    const pageSize = params.get("pageSize");

    return {
        pageIndex: pageIndex ? parseInt(pageIndex, 10) - 1 : 0,
        pageSize: pageSize ? parseInt(pageSize, 10) : 20,
        name: params.get("searchTerm") || undefined,
        description: params.get("searchTerm") || undefined,
        type: params.get("searchTerm") || undefined,
        // searchTerm: params.get("searchTerm") || undefined,
    }
}