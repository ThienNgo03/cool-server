import { useState } from "react"
import { useGetExercises, type GetExerciseRequest } from "./_apis/get-exercise"
import { Button } from "@/components/ui/button"
import { RotateCcw } from "lucide-react"
import { GridView } from "./_components/grid-view"
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs"

export default function Page() {
    const [request, setRequest] = useState<GetExerciseRequest>({ pageSize: 10, pageIndex: 0 })
    const { data, refetch, isLoading } = useGetExercises(request)

    return (
        <div>
            <Tabs defaultValue="data">
                <TabsList>
                    <TabsTrigger value="data">Data</TabsTrigger>
                    <TabsTrigger value="schema">Schema</TabsTrigger>
                </TabsList>

                <TabsContent value="data">
                    <div className="flex justify-between items-center">

                        <h1 className="font-bold text-xl">
                            Exercise Page
                        </h1>
                        <div className="flex gap-2">
                            <Button onClick={() => { refetch() }}>
                                <RotateCcw className="h-4 w-4" />
                                Refetch
                            </Button>
                        </div>
                    </div>

                    <GridView
                        data={data?.items || []}
                        isLoading={isLoading}
                        query={request}
                        onQueryChange={setRequest}
                    />
                </TabsContent>
                <TabsContent value="schema">Data Schema</TabsContent>
            </Tabs>
        </div>
    )
}