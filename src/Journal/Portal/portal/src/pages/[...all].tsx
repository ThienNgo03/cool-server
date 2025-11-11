import { useNavigate } from "react-router"
import image from "../assets/not-found.svg"
import { Button } from "@/components/ui/button"
import {
    Empty,
    EmptyContent,
    EmptyDescription,
    EmptyHeader,
    EmptyMedia,
    EmptyTitle,
} from "@/components/ui/empty"

export default function Page() {
    const navigate = useNavigate()
    return (
        <Empty className="h-full">
            <EmptyHeader>
                <EmptyMedia variant="default">
                    <img className="w-[200px] h-[200px]" src={image} alt="Page Icon" />
                </EmptyMedia>
                <EmptyTitle>Page not found</EmptyTitle>
                <EmptyDescription>
                    Sorry, we couldn&apos;t find the page you&apos;re looking for.
                </EmptyDescription>
            </EmptyHeader>
            <EmptyContent>
                <div className="flex gap-2" onClick={() => navigate("/")}>
                    <Button>Go to homepage</Button>
                </div>
            </EmptyContent>
        </Empty>
    )
}