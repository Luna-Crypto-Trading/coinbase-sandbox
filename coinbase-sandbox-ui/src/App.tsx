import { BrowserRouter, Route, Routes } from "react-router-dom";
import { Box, CssBaseline } from "@mui/material";
import { useState } from "react";
import "./App.css";
import { AppSidebar } from "./components/AppSidebar";
import { Dashboard } from "./components/Dashboard";
import { AppHeader } from "./components/AppHeader";
import { Portfolio } from "./components/Portfolio";
import { WebSocketDebug } from "./components/WebSocketDebug";

function App() {
  const [sidebarOpen, setSidebarOpen] = useState(true);

  const handleToggleSidebar = () => {
    setSidebarOpen(!sidebarOpen);
  };

  return (
    <BrowserRouter>
      <Box sx={{ display: "flex" }}>
        <CssBaseline />
        <AppHeader
          sidebarOpen={sidebarOpen}
          onToggleSidebar={handleToggleSidebar}
        />
        <AppSidebar open={sidebarOpen} onClose={() => setSidebarOpen(false)} />
        <Box
          component="main"
          sx={{
            flexGrow: 1,
            pt: 3,
            pr: 3,
            pb: 3,
            pl: 0,
            width: "100%",
            transition: (theme) =>
              theme.transitions.create(["margin", "width"], {
                easing: theme.transitions.easing.sharp,
                duration: theme.transitions.duration.leavingScreen,
              }),
          }}
        >
          <Routes>
            <Route path="/" element={<Dashboard />} />
            <Route path="/portfolio" element={<Portfolio />} />
            <Route path="/debug" element={<WebSocketDebug />} />
          </Routes>
        </Box>
      </Box>
    </BrowserRouter>
  );
}

export default App;
