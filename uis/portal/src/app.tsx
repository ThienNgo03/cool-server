// @ts-expect-error: react router pages
import routes from '~react-pages'
import { useRoutes } from 'react-router-dom'
import { Suspense } from 'react'
import { Spinner } from './components/ui/spinner'
import { Empty, EmptyHeader, EmptyMedia, EmptyTitle } from './components/ui/empty'
import { Layouts } from './layouts'

export function App() {
    return (
        <Suspense fallback={
            <Empty className="h-full">
                <EmptyHeader>
                    <EmptyMedia variant="default">
                        <Spinner className="size-8 text-primary" />
                    </EmptyMedia>
                    <EmptyTitle className='text-gray-500'>
                        Loading page...
                    </EmptyTitle>
                </EmptyHeader>
            </Empty>
        }>
            <Layouts>
                {useRoutes(routes)}
            </Layouts>
        </Suspense>
    )
}