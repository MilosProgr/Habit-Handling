// App.tsx
import { useEffect } from "react";
import "./App.css";
import { ReactKeycloakProvider, useKeycloak } from "@react-keycloak/web";
import keycloak from "./keycloak";
import Habits from "./features/habits/habits";
import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";
import MainLayout from "./components/MainLayout";
import Dashboard from "./pages/DashBoard";
import { EntriesPage } from "./features/entries/EntriesPage";
import HabitCreatePage from "./features/habits/createHabit";
import CreateEntriesPage from "./features/entries/CreateEntriesPage";
import { ProfilePage } from "./pages/Profile";
function App() {
  return (
    <ReactKeycloakProvider
      authClient={keycloak}
      initOptions={{
        onLoad: "login-required", // forsira login
        checkLoginIframe: false,   // sprečava refresh loop
        pkceMethod: "S256",
      }}
    >
      <BrowserRouter>
        <SecuredContent />
      </BrowserRouter>
    </ReactKeycloakProvider>
  );
}

const SecuredContent = () => {
  const { keycloak } = useKeycloak();
  const isLoggedIn = keycloak.authenticated;

  useEffect(() => {
    if (isLoggedIn === false) keycloak?.login();
  }, [isLoggedIn, keycloak]);

  if (!isLoggedIn) return <div>Loading...</div>;

  return (
    <Routes>
      {/* Layout kao “okvir” */}
      <Route path="/" element={<MainLayout />}>
        {/* Dashboard kao default */}
        <Route index element={<Dashboard />} />
        <Route path="dashboard" element={<Dashboard />} />
        <Route path="habits" element={<Habits />} />
        <Route path="habits/create" element={<HabitCreatePage />} />
        <Route path="entries" element={<EntriesPage />} />
        <Route path="entries/create" element={<CreateEntriesPage />} />
        <Route path="tags" element={<div>Tags page</div>} />
        <Route path="profile" element={<ProfilePage />} />
        {/* Ako neko unese random URL */}
        <Route path="*" element={<Navigate to="/dashboard" replace />} />
      </Route>
    </Routes>
  );
};



export default App;
