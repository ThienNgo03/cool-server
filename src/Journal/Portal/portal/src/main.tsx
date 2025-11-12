import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { createRoot } from 'react-dom/client'
import { StrictMode } from 'react'
import { BrowserRouter } from 'react-router-dom'
import { ThemeProvider } from "next-themes"
import { Toaster } from './components/ui/sonner';
import { App } from './app';
import './index.css'

const queryClient = new QueryClient({
    defaultOptions: {
        queries: {
            refetchOnWindowFocus: false,
            retry: false
        },
    }
})

createRoot(document.getElementById('root')!).render(
    <StrictMode>
        <ThemeProvider defaultTheme="system" attribute="class">
            <BrowserRouter>
                <QueryClientProvider client={queryClient}>
                    <App />
                </QueryClientProvider>
            </BrowserRouter>
            <Toaster toastOptions={{ duration: 5000, descriptionClassName: "!text-foreground" }} />
        </ThemeProvider>
    </StrictMode>
)
