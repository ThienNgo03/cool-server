import { defineConfig } from "vite"
import path from "path"
import tailwindcss from "@tailwindcss/vite"
import react from "@vitejs/plugin-react"
import Pages from "vite-plugin-pages"

export default defineConfig({
    plugins: [
        react(),
        tailwindcss(),
        Pages({
            extensions: ['tsx'],
            importMode: 'async',
            routeStyle: 'next',
            resolver: 'react',
            exclude: ['**/_*'],
            extendRoute(route) {
                if (route.path) {
                    route.path = route.path.replace(/\(.*?\)/g, '')
                    route.path = route.path.replace(/\/+/g, '/')
                }
                return route
            },
        }),
    ],
    resolve: {
        alias: {
            "@": path.resolve(__dirname, "./src"),
        },
    },
})