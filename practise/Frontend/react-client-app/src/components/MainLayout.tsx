// src/components/MainLayout.tsx
import Sidebar from "../components/SideBar";
import TopBar from "../components/TopBar";
import { Outlet } from "react-router-dom";

const MainLayout = () => {
    return (
        <div style={{ display: "flex" }}>
            <Sidebar />
            <main style={{ flex: 1, padding: "20px" }}>
                <TopBar />
                <Outlet />
            </main>
        </div>
    );
};

export default MainLayout;
