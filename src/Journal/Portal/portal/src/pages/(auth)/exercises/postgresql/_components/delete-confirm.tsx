import { toast } from "sonner";
import { useState } from "react";
import { useDeleteExercise } from "../_apis/delete-exercise";
import {
    AlertDialog,
    AlertDialogAction,
    AlertDialogCancel,
    AlertDialogContent,
    AlertDialogDescription,
    AlertDialogFooter,
    AlertDialogHeader,
    AlertDialogTitle,
} from "@/components/ui/alert-dialog"
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Copy } from "lucide-react";
import { Switch } from "@/components/ui/switch";

type DeleteConfirmProps = {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    exerciseId: string;
}

export function DeleteConfirm({ open, onOpenChange, exerciseId }: DeleteConfirmProps) {
    const [confirmId, setConfirmId] = useState("");
    const [isDeleteWorkouts, setIsDeleteWorkouts] = useState(false);
    const { mutate, } = useDeleteExercise();

    function onCopyClick() {
        navigator.clipboard.writeText(exerciseId);
    }

    function onDialogOpenChange(open: boolean) {
        if (!open) {
            setConfirmId("");
            setIsDeleteWorkouts(false);
        }
        onOpenChange(open);
    }

    function onDelete() {
        mutate(
            { exerciseId, isDeleteWorkouts },
            {
                onSuccess() {
                    toast.success("Exercise deleted successfully", {
                        description: "Exercise deleted successfully",
                    });
                },
                onError(error) {
                    toast.error("Error deleting exercise", {
                        description: error instanceof Error ? error.message : "Unknown error",
                    });
                }
            }
        );
    }

    return (
        <AlertDialog open={open} onOpenChange={onDialogOpenChange} key={exerciseId}>
            <AlertDialogContent>
                <AlertDialogHeader>
                    <AlertDialogTitle>Confirm Deletion</AlertDialogTitle>
                    <AlertDialogDescription asChild>
                        <div>
                            This action is irreversible.
                            <br />
                            The entity with ID
                            <span className="font-bold"> {exerciseId}
                                <Button variant={"ghost"} size={"icon"} className="w-6 h-6" onClick={onCopyClick}>
                                    <Copy className="h-4 w-4" />
                                </Button>
                            </span>
                            will be permanently deleted from our servers.
                            <br />
                            <div className="flex items-center my-4 gap-2">
                                <Switch checked={isDeleteWorkouts} onCheckedChange={setIsDeleteWorkouts} /> Also delete associated workouts
                            </div>

                            To proceed, please re-enter the <span className="font-bold">ID</span> below:
                            <br />
                            <Input value={confirmId} onChange={(e) => setConfirmId(e.target.value)} />
                        </div>
                    </AlertDialogDescription>
                </AlertDialogHeader>
                <AlertDialogFooter>
                    <AlertDialogCancel>No</AlertDialogCancel>
                    <AlertDialogAction
                        disabled={confirmId !== exerciseId}
                        onClick={onDelete}
                    >
                        Yes
                    </AlertDialogAction>
                </AlertDialogFooter>
            </AlertDialogContent>
        </AlertDialog>
    )
}