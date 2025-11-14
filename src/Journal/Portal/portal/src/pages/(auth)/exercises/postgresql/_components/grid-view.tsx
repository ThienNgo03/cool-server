import { type ColDef, type ICellRendererParams, type ValueFormatterParams } from 'ag-grid-community';
import { type CustomCellRendererProps } from 'ag-grid-react';
import type { IExercise } from '@/interfaces/models/exercise';

import { format } from 'date-fns';
import { useSearchParams } from 'react-router';
import { useEffect, useMemo, useState } from 'react';
import { useListExercise } from '../_hooks/use-list-exercise';

import { Badge } from '@/components/ui/badge';
import { BaseGrid } from '@/components/base/base-grid';
import { GridActions } from './grid/grid-actions';
import { DeleteConfirm } from './delete-confirm';

import { GridToolbar } from './grid/grid-toolbar';
import { GridPagination } from './grid/grid-pagination';
import { useUpdateUrlParams } from '@/lib/updateUrlParams';

const descriptionCellStyle = {
    'whiteSpace': 'normal',
    'lineHeight': '1.6',
    "paddingTop": "8px",
    "paddingBottom": "8px",
    "overflow": "hidden !important",
    "display": "-webkit-box !important",
    "textOverflow": "ellipsis",
    "WebkitBoxOrient": "vertical",
    "WebkitLineClamp": 3,
}

const config = {
    pageIndex: 1,
    pageSize: 20,
}

export function GridView() {
    const [searchParams] = useSearchParams();
    const { update } = useUpdateUrlParams();
    const { data, refetch, isLoading } = useListExercise();
    const [openDeleteDialog, setOpenDeleteDialog] = useState(false);
    const [selectedExerciseId, setSelectedExerciseId] = useState<string | null>(null);

    const colDefs = useMemo<ColDef<IExercise>[]>(() => [
        {
            field: "id",
            minWidth: 200,
        },
        {
            field: "name",
            width: 150,
            flex: 1
        },
        {
            field: "description",
            width: 400,
            cellStyle: descriptionCellStyle,
            flex: 2
        },
        {
            field: "type",
            width: 150,
            cellRenderer: (params: CustomCellRendererProps) => {
                return (
                    <Badge>{params.value}</Badge>
                )
            }
        },
        {
            field: "createdDate",
            width: 150,
            valueFormatter: (params: ValueFormatterParams) => {
                return format(params.value, "dd/MM/yyyy");
            },
        },
        {
            colId: "actions",
            headerName: "Actions",
            width: 100,
            cellRenderer: GridActions,
            cellRendererParams: (params: ICellRendererParams) => ({
                onClick: () => {
                    setOpenDeleteDialog(true);
                    setSelectedExerciseId(params.data.id);
                },
                params,
            }),
        }
    ], []);

    useEffect(() => {
        const pageIndex = searchParams.get("pageIndex");
        const pageSize = searchParams.get("pageSize");
        update({
            pageIndex: pageIndex ?? config.pageIndex.toString(),
            pageSize: pageSize ?? config.pageSize.toString()
        });
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, []);

    return (
        <div>
            <GridToolbar refetch={refetch} />
            <div className="h-[calc(100vh-210px)] my-2">
                <BaseGrid<IExercise>
                    data={data?.items || []}
                    isLoading={isLoading}
                    colDefs={colDefs}
                    rowHeight={82}
                />
            </div>
            <GridPagination totalItems={data?.all || 0} />
            {
                selectedExerciseId && (
                    <DeleteConfirm open={openDeleteDialog} onOpenChange={setOpenDeleteDialog} exerciseId={selectedExerciseId} />
                )
            }
        </div>
    )
}