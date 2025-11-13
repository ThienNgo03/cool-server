import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import debounce from "lodash.debounce";
import { Plus, RotateCcw, X } from "lucide-react";
import { useEffect, useMemo, useState } from "react";
import { useNavigate, useSearchParams } from "react-router";
import { useUpdateUrlParams } from "@/lib/updateUrlParams";

type GridToolbarProps = {
    refetch: () => void;
}

export function GridToolbar({ refetch }: GridToolbarProps) {
    const navigate = useNavigate();
    const [searchParams] = useSearchParams();
    const { update } = useUpdateUrlParams();
    const searchTerm = searchParams.get("searchTerm") || "";
    const [searchValue, setSearchValue] = useState<string>(searchTerm);

    const onSearchChange = (value: string) => {
        setSearchValue(value);
        debouncedSearchChange(value);
    }

    const debouncedSearchChange = useMemo(() => {
        return debounce((value: string) => {
            update({ searchTerm: value });
        }, 700);
    }, [update]);

    useEffect(() => {
        return () => {
            debouncedSearchChange.cancel();
        };
    }, [debouncedSearchChange]);

    return (
        <div className="flex justify-between items-center">
            <div className="relative w-full max-w-sm">
                <Input
                    placeholder="Search exercises..."
                    className="max-w-sm"
                    value={searchValue}
                    onChange={(e) => onSearchChange(e.target.value)}
                />
                {
                    searchValue && (
                        <Button
                            variant="ghost"
                            size="icon"
                            className="absolute right-1 top-1/2 -translate-y-1/2 h-6 w-6 rounded-full"
                            onClick={() => onSearchChange("")}
                        >
                            <X className="h-4 w-4" />
                        </Button>
                    )
                }
            </div>

            <div className="flex gap-2">
                <Button variant="outline" onClick={() => refetch()}>
                    <RotateCcw className="h-4 w-4" />
                    Refresh
                </Button>
                <Button onClick={() => navigate("/exercises/create")}>
                    <Plus className="h-4 w-4" />
                    Create
                </Button>
            </div>
        </div>
    )
}