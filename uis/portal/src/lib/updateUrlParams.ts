import { useCallback } from "react";
import { useSearchParams } from "react-router-dom";

export function useUpdateUrlParams() {
    const [searchParams, setSearchParams] = useSearchParams();

    const update = useCallback((params: Record<string, string | undefined>) => {
        const updatedParams = new URLSearchParams(searchParams.toString());
        Object.entries(params).forEach(([key, value]) => {
            if (value !== undefined && value !== null && value !== "") {
                updatedParams.set(key, value);
            } else {
                updatedParams.delete(key);
            }
        });
        setSearchParams(updatedParams);
    }, [searchParams, setSearchParams]);

    return { update };
}
