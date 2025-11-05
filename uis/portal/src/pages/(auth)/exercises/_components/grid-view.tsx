import * as React from "react"
import {
    flexRender,
    getCoreRowModel,
    getFilteredRowModel,
    getPaginationRowModel,
    getSortedRowModel,
    useReactTable,
    type ColumnDef,
    type SortingState,
    type VisibilityState,
} from "@tanstack/react-table"
import { ArrowUpDown, ChevronDown, MoreHorizontal } from "lucide-react"

import { Button } from "@/components/ui/button"
import { Checkbox } from "@/components/ui/checkbox"
import {
    DropdownMenu,
    DropdownMenuCheckboxItem,
    DropdownMenuContent,
    DropdownMenuItem,
    DropdownMenuLabel,
    DropdownMenuSeparator,
    DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu"
import { Input } from "@/components/ui/input"
import {
    Table,
    TableBody,
    TableCell,
    TableHead,
    TableHeader,
    TableRow,
} from "@/components/ui/table"

import { Badge } from "@/components/ui/badge"
import type { IExercise } from "@/interfaces/models/exercise";
import type { GetExerciseRequest } from "../_apis/get-exercise"
import debounce from "lodash.debounce";
import { format } from "date-fns"
import { useNavigate } from "react-router"

const columns: ColumnDef<IExercise>[] = [
    {
        id: "select",
        header: ({ table }) => (
            <Checkbox
                checked={
                    table.getIsAllPageRowsSelected() ||
                    (table.getIsSomePageRowsSelected() && "indeterminate")
                }
                onCheckedChange={(value) => table.toggleAllPageRowsSelected(!!value)}
                aria-label="Select all"
            />
        ),
        cell: ({ row }) => (
            <Checkbox
                checked={row.getIsSelected()}
                onCheckedChange={(value) => row.toggleSelected(!!value)}
                aria-label="Select row"
            />
        ),
        enableSorting: false,
        enableHiding: false,
    },
    {
        accessorKey: "id",
        header: "ID",
        cell: ({ row }) => (
            <div className="text-wrap">{row.getValue("id")}</div>
        ),
    },
    {
        accessorKey: "name",
        header: ({ column }) => {
            return (
                <Button
                    variant="ghost"
                    onClick={() => column.toggleSorting(column.getIsSorted() === "asc")}
                >
                    Name
                    <ArrowUpDown />
                </Button>
            )
        },
        cell: ({ row }) => (
            <Badge>
                {row.getValue("name")}
            </Badge>
        ),
        enableSorting: true
    },
    {
        accessorKey: "description",
        header: "Description",
        cell: ({ row }) => (
            <div className="text-wrap line-clamp-3">{row.getValue("description")}</div>
        ),
    },
    {
        accessorKey: "type",
        header: ({ column }) => {
            return (
                <Button
                    variant="ghost"
                    onClick={() => column.toggleSorting(column.getIsSorted() === "asc")}
                >
                    Type
                    <ArrowUpDown />
                </Button>
            )
        },
        cell: ({ row }) => (
            <div className="capitalize">{row.getValue("type")}</div>
        ),
        enableSorting: true
    },
    {
        accessorKey: "createdDate",
        header: "Created Date",
        cell: ({ row }) => (
            <div>{format(new Date(row.getValue("createdDate")), "dd/MM/yyyy")}</div>
        ),
    },
    {
        accessorKey: "updatedDate",
        header: "Updated Date",
        cell: ({ row }) => {
            const updatedDate = row.getValue("updatedDate");
            if (!updatedDate) {
                return <div>N/A</div>;
            }
            return <div>{format(new Date(row.getValue("updatedDate")), "PPP")}</div>;
        }
    },
    {
        id: "actions",
        enableHiding: false,
        cell: ({ row }) => {
            const exercise = row.original;
            return (
                <DropdownMenu>
                    <DropdownMenuTrigger asChild>
                        <Button variant="ghost" className="h-8 w-8 p-0">
                            <span className="sr-only">Open menu</span>
                            <MoreHorizontal />
                        </Button>
                    </DropdownMenuTrigger>
                    <DropdownMenuContent align="end">
                        <DropdownMenuLabel>Actions</DropdownMenuLabel>
                        <DropdownMenuItem
                            onClick={() => navigator.clipboard.writeText(exercise.id)}
                        >
                            Copy exercise ID
                        </DropdownMenuItem>
                        <DropdownMenuSeparator />
                        <DropdownMenuItem>
                            View details
                        </DropdownMenuItem>
                    </DropdownMenuContent>
                </DropdownMenu>
            )
        },
    },
]

type GridViewProps = {
    data: IExercise[];
    isLoading: boolean;
    query: GetExerciseRequest;
    onQueryChange: React.Dispatch<React.SetStateAction<GetExerciseRequest>>;
}

export function GridView({ data, query: { pageSize, pageIndex }, onQueryChange }: GridViewProps) {
    const [sorting, setSorting] = React.useState<SortingState>([])
    const [columnVisibility, setColumnVisibility] =
        React.useState<VisibilityState>({})
    const [rowSelection, setRowSelection] = React.useState({})
    const [filter, setFilter] = React.useState("");
    const [pagination, setPagination] = React.useState({
        pageIndex: pageIndex ?? 0, //initial page index
        pageSize: pageSize ?? 10, //default page size
    });

    const table = useReactTable({
        data,
        columns,
        onSortingChange: setSorting,
        getCoreRowModel: getCoreRowModel(),
        getPaginationRowModel: getPaginationRowModel(),
        getSortedRowModel: getSortedRowModel(),
        getFilteredRowModel: getFilteredRowModel(),
        onColumnVisibilityChange: setColumnVisibility,
        onRowSelectionChange: setRowSelection,
        state: {
            sorting,
            columnVisibility,
            rowSelection,
            pagination
        },
    });
    const navigate = useNavigate();

    const debouncedSearchChange = React.useMemo(() => {
        return debounce((value: string) => {
            onQueryChange((prev) => ({
                ...prev,
                name: value,
                description: value,
                type: value,
            }))
        }, 700);
    }, [onQueryChange]);


    // useEffect(() => {
    //     let connection: ReturnType<typeof getConnection> | undefined;
    //     const setupSignalR = async () => {
    //         const conn = await startConnection();
    //         conn.on('{event-name}', () => {
    //           refetch();
    //         });
    //         return conn;
    //     };
    //     setupSignalR().then((conn) => (connection = conn));
    //     return () => {
    //         connection?.off('{event-name}');
    //     };
    // }, []);

    React.useEffect(() => {
        return () => {
            debouncedSearchChange.cancel();
        };
    }, [debouncedSearchChange]);

    return (
        <div className="w-full">
            <div className="flex items-center py-4">
                <Input
                    placeholder="Filter text..."
                    value={filter}
                    onChange={(event) => {
                        setFilter(event.target.value)
                        debouncedSearchChange(event.target.value)
                    }}
                    className="max-w-sm"
                />
                <DropdownMenu>
                    <DropdownMenuTrigger asChild>
                        <Button variant="outline" className="ml-auto">
                            Columns <ChevronDown />
                        </Button>
                    </DropdownMenuTrigger>
                    <DropdownMenuContent align="end">
                        {table
                            .getAllColumns()
                            .filter((column) => column.getCanHide())
                            .map((column) => {
                                return (
                                    <DropdownMenuCheckboxItem
                                        key={column.id}
                                        className="capitalize"
                                        checked={column.getIsVisible()}
                                        onCheckedChange={(value) =>
                                            column.toggleVisibility(!!value)
                                        }
                                    >
                                        {column.id}
                                    </DropdownMenuCheckboxItem>
                                )
                            })}
                    </DropdownMenuContent>
                </DropdownMenu>
            </div>
            <div className="overflow-hidden rounded-md border">
                <Table>
                    <TableHeader>
                        {table.getHeaderGroups().map((headerGroup) => (
                            <TableRow key={headerGroup.id}>
                                {headerGroup.headers.map((header) => {
                                    return (
                                        <TableHead key={header.id}>
                                            {header.isPlaceholder
                                                ? null
                                                : flexRender(
                                                    header.column.columnDef.header,
                                                    header.getContext()
                                                )}
                                        </TableHead>
                                    )
                                })}
                            </TableRow>
                        ))}
                    </TableHeader>
                    <TableBody>
                        {table.getRowModel().rows?.length ? (
                            table.getRowModel().rows.map((row) => (
                                <TableRow
                                    key={row.id}
                                    data-state={row.getIsSelected() && "selected"}
                                    onClick={() => navigate(`/exercises/${row.original.id}`)}
                                >
                                    {row.getVisibleCells().map((cell) => (
                                        <TableCell key={cell.id}>
                                            {flexRender(
                                                cell.column.columnDef.cell,
                                                cell.getContext()
                                            )}
                                        </TableCell>
                                    ))}
                                </TableRow>
                            ))
                        ) : (
                            <TableRow>
                                <TableCell
                                    colSpan={columns.length}
                                    className="h-24 text-center"
                                >
                                    No results.
                                </TableCell>
                            </TableRow>
                        )}
                    </TableBody>
                </Table>
            </div>
            <div className="flex items-center justify-end space-x-2 py-4">
                <div className="text-muted-foreground flex-1 text-sm">
                    Page Size:
                    <Input
                        type="number"
                        value={pageSize}
                        onChange={(e) => {

                            onQueryChange((prev) => ({
                                ...prev,
                                pageSize: Number(e.target.value),
                                pageIndex: 0,
                            }))
                            setPagination((prev) => ({
                                ...prev,
                                pageSize: Number(e.target.value),
                                pageIndex: 0,
                            }))
                        }}
                        className="w-16 mr-2"
                    />
                    Page {(pageIndex ?? 0) + 1} Size {pageSize}
                </div>
                <div className="space-x-2">
                    <Button
                        variant="outline"
                        size="sm"
                        onClick={() => onQueryChange((prev) => ({ ...prev, pageIndex: Math.max((pageIndex ?? 0) - 1, 0) }))}
                    >
                        Previous
                    </Button>
                    <Button
                        variant="outline"
                        size="sm"
                        onClick={() => onQueryChange((prev) => ({ ...prev, pageIndex: Math.max((pageIndex ?? 0) + 1, 0) }))}
                    >
                        Next
                    </Button>
                </div>
            </div>
        </div>
    )
}
