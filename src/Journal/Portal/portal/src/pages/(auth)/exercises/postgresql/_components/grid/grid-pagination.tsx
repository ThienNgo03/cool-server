import { Input } from "@/components/ui/input";
import {
    Pagination,
    PaginationContent,
    PaginationEllipsis,
    PaginationItem,
    PaginationLink,
    PaginationNext,
    PaginationPrevious,
} from "@/components/ui/pagination"
import { useUpdateUrlParams } from "@/lib/updateUrlParams";
import debounce from "lodash.debounce";
import { useEffect, useMemo, useState } from "react";
import { useSearchParams } from "react-router-dom";

type GetPaginationProps = {
    totalItems: number;
}

export function GridPagination({ totalItems }: GetPaginationProps) {
    const [searchParams] = useSearchParams();
    const { update } = useUpdateUrlParams();
    const pageSize = parseInt(searchParams.get("pageSize") || "20", 10);
    const currentPage = parseInt(searchParams.get("pageIndex") || "1", 10);
    const [sizeInput, setSizeInput] = useState(pageSize.toString());

    const onInputChange = (value: string) => {
        setSizeInput(value);
        debouncedInputChange(value);
    }

    const debouncedInputChange = useMemo(() => {
        return debounce((value: string) => {
            update({ pageSize: value, pageIndex: "1" });
        }, 700);
    }, [update]);

    useEffect(() => {
        return () => {
            debouncedInputChange.cancel();
        };
    }, [debouncedInputChange]);

    const totalPages = useMemo(() => {
        return Math.ceil(totalItems / pageSize)
    }, [totalItems, pageSize]);

    const siblingCount = 2;

    const paginationItems = useMemo(() => {
        const pages: (number | string)[] = [];

        const startPage = Math.max(1, currentPage - siblingCount);
        const endPage = Math.min(totalPages, currentPage + siblingCount);

        // Always show first page
        if (startPage > 1) {
            pages.push(1);
            if (startPage > 2) pages.push("…");
        }

        // Middle pages
        for (let i = startPage; i <= endPage; i++) {
            pages.push(i);
        }

        // Always show last page
        if (endPage < totalPages) {
            if (endPage < totalPages - 1) pages.push("…");
            pages.push(totalPages);
        }

        return pages.map((page, i) => {
            if (page === "…") {
                return (
                    <PaginationEllipsis key={`ellipsis-${i}`} />
                );
            }

            return (
                <PaginationItem key={page}>
                    <PaginationLink
                        className="cursor-pointer"
                        isActive={currentPage === page}
                        onClick={() => update({ pageIndex: page.toString() })}
                    >
                        {page}
                    </PaginationLink>
                </PaginationItem>
            );
        });
    }, [currentPage, totalPages, siblingCount, update]);

    return (
        <div className="flex flex-row justify-between items-center">
            <p className="text-sm text-muted-foreground">
                Showing page {currentPage} of {totalPages} ({totalItems} items)
            </p>
            <Pagination className="flex-0">
                <PaginationContent>
                    <PaginationItem>
                        <PaginationPrevious
                            className="cursor-pointer"
                            onClick={() => update({ pageIndex: Math.max(currentPage - 1, 1).toString() })}
                        />
                    </PaginationItem>
                    {paginationItems}
                    <PaginationItem>
                        <PaginationNext
                            className="cursor-pointer"
                            onClick={() => update({ pageIndex: Math.min(currentPage + 1, totalPages).toString() })}
                        />
                    </PaginationItem>
                </PaginationContent>
            </Pagination >
            <div className="flex items-center space-x-2 w-25">
                <span className="text-sm text-muted-foreground">
                    Size:
                </span>
                <Input className="text-right" value={sizeInput} onChange={(e) => onInputChange(e.target.value)} />
            </div>
        </div>
    )
}