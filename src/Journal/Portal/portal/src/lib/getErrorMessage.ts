import axios from "axios";

export function getErrorMessage(error: unknown): string {
    if (axios.isAxiosError(error)) {
        const data = error.response?.data;
        if (data && typeof data === "string") {
            return data;
        }

        return error.response?.data?.message || error.message || "An unexpected error occurred";
    }

    if (error instanceof Error) {
        return error.message;
    }

    return String(error);
}