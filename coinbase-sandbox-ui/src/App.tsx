import React, { useState } from "react";
import {
  ThemeProvider,
  createTheme,
  CssBaseline,
  Typography,
  Box,
  AppBar,
  Toolbar,
  IconButton,
} from "@mui/material";
import MenuIcon from "@mui/icons-material/Menu";
import { Sidebar } from "./components/common/Sidebar";

const drawerWidth = 260;

// Create a theme instance
const theme = createTheme({
  palette: {
    mode: "dark",
    primary: {
      main: "#0052ff",
    },
    secondary: {
      main: "#00c853",
    },
    background: {
      default: "#121212",
      paper: "#1e1e1e",
    },
  },
  typography: {
    fontFamily: '"Roboto", "Helvetica", "Arial", sans-serif',
  },
  components: {
    MuiPaper: {
      styleOverrides: {
        root: {
          backgroundImage: "none",
        },
      },
    },
  },
});

const App: React.FC = () => {
  const [sidebarOpen, setSidebarOpen] = useState(true);

  const handleSidebarToggle = () => {
    setSidebarOpen((open) => !open);
  };

  return (
    <ThemeProvider theme={theme}>
      <CssBaseline />
      <Box sx={{ display: "flex" }}>
        <Sidebar open={sidebarOpen} onClose={handleSidebarToggle} />
        <AppBar
          position="fixed"
          color="default"
          elevation={0}
          sx={{
            left: sidebarOpen ? `${drawerWidth}px` : 0,
            width: sidebarOpen ? `calc(100% - ${drawerWidth}px)` : "100%",
            boxShadow: "none",
            borderBottom: "1px solid",
            borderColor: "divider",
            zIndex: (theme) => theme.zIndex.drawer + 1,
            transition: (theme) =>
              theme.transitions.create(["left", "width"], {
                easing: theme.transitions.easing.sharp,
                duration: theme.transitions.duration.leavingScreen,
              }),
          }}
        >
          <Toolbar sx={{ minHeight: 64 }}>
            <IconButton
              color="inherit"
              aria-label={sidebarOpen ? "hide sidebar" : "show sidebar"}
              edge="start"
              onClick={handleSidebarToggle}
              sx={{ mr: 2 }}
            >
              <MenuIcon />
            </IconButton>
            {/* You can add more header content here if needed */}
          </Toolbar>
        </AppBar>
        <Box
          component="main"
          sx={{
            flexGrow: 1,
            p: 3,
            width: "100%",
            mt: 8,
            transition: (theme) =>
              theme.transitions.create(["margin"], {
                easing: theme.transitions.easing.sharp,
                duration: theme.transitions.duration.leavingScreen,
              }),
          }}
        >
          <Typography paragraph>
            Welcome to Coinbase Sandbox Dashboard
          </Typography>
        </Box>
      </Box>
    </ThemeProvider>
  );
};

export default App;
