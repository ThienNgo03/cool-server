import { useParams } from "react-router"
import { useGetExercises } from "./_apis/get-exercise";

export default function Page() {
    const { id } = useParams<{ id: string }>();
    const { data } = useGetExercises({ ids: id, include: 'muscles' });

    if (data?.items.length === 0) {
        return <div>Exercise not found</div>
    }

    return <div>{JSON.stringify(data?.items)}</div>
}