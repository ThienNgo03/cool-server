import type { CustomNoRowsOverlayProps } from "ag-grid-react";

export function NoRowOverlay(props: CustomNoRowsOverlayProps) {
    return (
        <div className="flex flex-col items-center justify-center h-full">
            <p className="text-sm text-gray-500">No data available</p>
            <p className="sr-only">
                {props.context?.message}
            </p>
        </div>
    );
}