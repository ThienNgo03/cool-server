import { z } from "zod";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";

const schema = z.object({
    name: z.string().min(1, "Name is required"),
    description: z.string().min(1, "Description is required"),
    type: z.string().min(1, "Type is required"),
})

export type ExerciseSchemaType = z.infer<typeof schema>;

export function useCreateForm() {
    const initialValues: ExerciseSchemaType = {
        name: "",
        description: "",
        type: "",
    };

    const form = useForm<ExerciseSchemaType>({
        resolver: zodResolver(schema),
        defaultValues: initialValues,
    });

    return { form };
}   