import { useState } from "react";
import {
  Drawer,
  List,
  ListItem,
  ListItemButton,
  ListItemIcon,
  ListItemText,
  Typography,
  Box,
  Chip,
  Collapse,
} from "@mui/material";
import {
  Home,
  TrendingUp,
  VpnKey,
  BarChart,
  AccountBalanceWallet,
  ShowChart,
  ExpandLess,
  ExpandMore,
  BugReport,
  DataUsage,
  Tune,
} from "@mui/icons-material";
import { Link, useLocation } from "react-router-dom";

const drawerWidth = 280;

const navigation = [
  {
    title: "Overview",
    items: [
      {
        title: "Dashboard",
        url: "/",
        icon: Home,
      },
      {
        title: "Portfolio",
        url: "/portfolio",
        icon: AccountBalanceWallet,
      },
    ],
  },
  {
    title: "Trading",
    items: [
      {
        title: "Trade",
        url: "/trading",
        icon: TrendingUp,
      },
      {
        title: "Order Book",
        url: "/orderbook",
        icon: BarChart,
      },
      {
        title: "Market Data",
        url: "/market",
        icon: DataUsage,
      },
    ],
  },
  {
    title: "Developer",
    items: [
      {
        title: "WebSocket Debug",
        url: "/debug",
        icon: BugReport,
      },
      {
        title: "Live Charts",
        url: "/live-charts",
        icon: ShowChart,
      },
    ],
  },
  {
    title: "Settings",
    items: [
      {
        title: "API Keys",
        url: "/settings",
        icon: VpnKey,
      },
      {
        title: "Preferences",
        url: "/preferences",
        icon: Tune,
      },
    ],
  },
];

interface AppSidebarProps {
  open: boolean;
  onClose: () => void;
}

export function AppSidebar({ open, onClose }: AppSidebarProps) {
  const location = useLocation();
  const [openSections, setOpenSections] = useState<Record<string, boolean>>({
    Overview: true,
    Trading: true,
    Developer: true,
    Settings: true,
  });

  const handleSectionToggle = (section: string) => {
    setOpenSections((prev) => ({
      ...prev,
      [section]: !prev[section],
    }));
  };

  return (
    <Drawer
      variant="permanent"
      open={open}
      onClose={onClose}
      sx={{
        width: open ? drawerWidth : 0,
        flexShrink: 0,
        transition: (theme) =>
          theme.transitions.create("width", {
            easing: theme.transitions.easing.sharp,
            duration: theme.transitions.duration.leavingScreen,
          }),
        "& .MuiDrawer-paper": {
          width: drawerWidth,
          boxSizing: "border-box",
          transition: (theme) =>
            theme.transitions.create("width", {
              easing: theme.transitions.easing.sharp,
              duration: theme.transitions.duration.leavingScreen,
            }),
          ...(open ? {} : { width: 0 }),
        },
      }}
    >
      <Box sx={{ p: 2, borderBottom: "1px solid #e0e0e0" }}>
        <Box sx={{ display: "flex", alignItems: "center", gap: 1, mb: 1 }}>
          <Box
            sx={{
              width: 32,
              height: 32,
              borderRadius: 1,
              bgcolor: "primary.main",
              display: "flex",
              alignItems: "center",
              justifyContent: "center",
              color: "white",
            }}
          >
            <TrendingUp fontSize="small" />
          </Box>
          <Typography variant="h6" component="div" sx={{ fontWeight: 600 }}>
            Coinbase Sandbox
          </Typography>
        </Box>
        <Chip
          label="Development"
          size="small"
          variant="outlined"
          sx={{ fontSize: "0.75rem" }}
        />
      </Box>

      <List sx={{ pt: 1 }}>
        {navigation.map((section) => (
          <Box key={section.title}>
            <ListItemButton onClick={() => handleSectionToggle(section.title)}>
              <ListItemText
                primary={section.title}
                primaryTypographyProps={{
                  fontSize: "0.875rem",
                  fontWeight: 600,
                  color: "text.secondary",
                }}
              />
              {openSections[section.title] ? <ExpandLess /> : <ExpandMore />}
            </ListItemButton>
            <Collapse
              in={openSections[section.title]}
              timeout="auto"
              unmountOnExit
            >
              <List component="div" disablePadding>
                {section.items.map((item) => (
                  <ListItem key={item.title} disablePadding>
                    <ListItemButton
                      component={Link}
                      to={item.url}
                      selected={location.pathname === item.url}
                      sx={{
                        pl: 4,
                        "&.Mui-selected": {
                          bgcolor: "primary.main",
                          color: "white",
                          "&:hover": {
                            bgcolor: "primary.dark",
                          },
                          "& .MuiListItemIcon-root": {
                            color: "white",
                          },
                        },
                      }}
                    >
                      <ListItemIcon sx={{ minWidth: 40 }}>
                        <item.icon fontSize="small" />
                      </ListItemIcon>
                      <ListItemText
                        primary={item.title}
                        primaryTypographyProps={{ fontSize: "0.875rem" }}
                      />
                    </ListItemButton>
                  </ListItem>
                ))}
              </List>
            </Collapse>
          </Box>
        ))}
      </List>
    </Drawer>
  );
}
