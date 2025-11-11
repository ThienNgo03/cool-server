import { Calendar, Home, Inbox, Search } from "lucide-react"

import {
    Sidebar,
    SidebarContent,
    SidebarFooter,
    SidebarGroup,
    SidebarGroupContent,
    SidebarGroupLabel,
    SidebarMenu,
    SidebarMenuButton,
    SidebarMenuItem,
} from "@/components/ui/sidebar"
import { ThemeToggle } from "./theme-toggle"

const items = [
    {
        title: "Home",
        url: "/",
        icon: Home,
    },
    {
        title: "Exercises",
        url: "/exercises",
        icon: Inbox,
    },
    {
        title: "Health",
        url: "/health",
        icon: Search,
    },
]

export function AppNavigation() {
    return (
        <Sidebar variant="floating" collapsible="icon" className="bg-background">
            <SidebarContent className="bg-background rounded-[8px]">
                <SidebarGroup>
                    <SidebarGroupLabel className="flex justify-between">
                        Portal
                        <ThemeToggle />
                    </SidebarGroupLabel>
                    <SidebarGroupContent>
                        <SidebarMenu>
                            {items.map((item) => (
                                <SidebarMenuItem key={item.title}>
                                    <SidebarMenuButton asChild>
                                        <a href={item.url}>
                                            <item.icon />
                                            <span>{item.title}</span>
                                        </a>
                                    </SidebarMenuButton>
                                </SidebarMenuItem>
                            ))}
                        </SidebarMenu>
                    </SidebarGroupContent>
                </SidebarGroup>
            </SidebarContent>
            <SidebarFooter className="bg-background rounded-[8px]">
                <SidebarMenu>
                    <SidebarMenuItem>
                        <SidebarMenuButton asChild>
                            <a href={"/about"}>
                                <Calendar />
                                <span>About</span>
                            </a>
                        </SidebarMenuButton>
                    </SidebarMenuItem>
                </SidebarMenu>
                <div className="text-center text-xs text-gray-500">
                    &copy; 2024 Cool Server
                </div>
            </SidebarFooter>
        </Sidebar>
    )
}