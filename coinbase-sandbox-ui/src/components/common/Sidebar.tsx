import React from "react";
import {
  Drawer,
  List,
  ListItemButton,
  ListItemIcon,
  ListItemText,
  ListSubheader,
  styled,
  Toolbar,
  Box,
  Typography,
  Chip,
} from "@mui/material";
import DashboardIcon from "@mui/icons-material/Dashboard";
import AccountBalanceIcon from "@mui/icons-material/AccountBalance";
import ShowChartIcon from "@mui/icons-material/ShowChart";
import SettingsIcon from "@mui/icons-material/Settings";
import TrendingUpIcon from "@mui/icons-material/TrendingUp";
import BarChartIcon from "@mui/icons-material/BarChart";
import TimelineIcon from "@mui/icons-material/Timeline";
import KeyIcon from "@mui/icons-material/VpnKey";
import TuneIcon from "@mui/icons-material/Tune";
import WebAssetIcon from "@mui/icons-material/WebAsset";
import InsertChartIcon from "@mui/icons-material/InsertChart";

const drawerWidth = 260;

const StyledDrawer = styled(Drawer)(({ theme }) => ({
  width: drawerWidth,
  flexShrink: 0,
  "& .MuiDrawer-paper": {
    width: drawerWidth,
    boxSizing: "border-box",
    backgroundColor: theme.palette.background.default,
    borderRight: `1px solid ${theme.palette.divider}`,
    paddingTop: 0,
  },
}));

interface SidebarProps {
  open: boolean;
  onClose: () => void;
}

export const Sidebar: React.FC<SidebarProps> = ({ open, onClose }) => {
  return (
    <StyledDrawer variant="persistent" open={open}>
      {/* Header */}
      <Toolbar
        sx={{
          minHeight: 80,
          px: 2,
          display: "flex",
          alignItems: "center",
          gap: 2,
          borderBottom: "1px solid",
          borderColor: "divider",
        }}
      >
        <Box
          sx={{
            bgcolor: "grey.900",
            p: 1,
            borderRadius: 2,
            display: "flex",
            alignItems: "center",
          }}
        >
          <TrendingUpIcon sx={{ color: "common.white", fontSize: 32 }} />
        </Box>
        <Box sx={{ flexGrow: 1 }}>
          <Typography
            variant="body1"
            fontWeight={700}
            sx={{ lineHeight: 1, mt: 0.8, mb: 0.5 }}
          >
            Coinbase Sandbox
          </Typography>
          <Chip
            label="Development"
            size="small"
            sx={{
              mt: 0.75,
              fontWeight: 500,
              bgcolor: "grey.100",
              color: "grey.800",
              fontSize: 10,
            }}
          />
        </Box>
      </Toolbar>
      {/* Menu */}
      <List
        sx={{ pt: 2, pb: 0, px: 2 }}
        subheader={
          <ListSubheader
            component="div"
            disableSticky
            sx={{
              bgcolor: "transparent",
              color: "grey.600",
              fontWeight: 600,
              fontSize: 14,
              mb: 1,
            }}
          >
            Overview
          </ListSubheader>
        }
      >
        <ListItemButton selected>
          <ListItemIcon>
            <DashboardIcon />
          </ListItemIcon>
          <ListItemText
            primary={<Typography fontWeight={600}>Dashboard</Typography>}
          />
        </ListItemButton>
        <ListItemButton>
          <ListItemIcon>
            <AccountBalanceIcon />
          </ListItemIcon>
          <ListItemText primary="Portfolio" />
        </ListItemButton>
      </List>
      <List
        sx={{ pt: 2, pb: 0, px: 2 }}
        subheader={
          <ListSubheader
            component="div"
            disableSticky
            sx={{
              bgcolor: "transparent",
              color: "grey.600",
              fontWeight: 600,
              fontSize: 14,
              mb: 1,
            }}
          >
            Trading
          </ListSubheader>
        }
      >
        <ListItemButton>
          <ListItemIcon>
            <ShowChartIcon />
          </ListItemIcon>
          <ListItemText primary="Trade" />
        </ListItemButton>
        <ListItemButton>
          <ListItemIcon>
            <BarChartIcon />
          </ListItemIcon>
          <ListItemText primary="Order Book" />
        </ListItemButton>
        <ListItemButton>
          <ListItemIcon>
            <TimelineIcon />
          </ListItemIcon>
          <ListItemText primary="Market Data" />
        </ListItemButton>
      </List>
      <List
        sx={{ pt: 2, pb: 0, px: 2 }}
        subheader={
          <ListSubheader
            component="div"
            disableSticky
            sx={{
              bgcolor: "transparent",
              color: "grey.600",
              fontWeight: 600,
              fontSize: 14,
              mb: 1,
            }}
          >
            Developer
          </ListSubheader>
        }
      >
        <ListItemButton>
          <ListItemIcon>
            <WebAssetIcon />
          </ListItemIcon>
          <ListItemText primary="WebSocket Debug" />
        </ListItemButton>
        <ListItemButton>
          <ListItemIcon>
            <InsertChartIcon />
          </ListItemIcon>
          <ListItemText primary="Live Charts" />
        </ListItemButton>
      </List>
      <List
        sx={{ pt: 2, pb: 0, px: 2 }}
        subheader={
          <ListSubheader
            component="div"
            disableSticky
            sx={{
              bgcolor: "transparent",
              color: "grey.600",
              fontWeight: 600,
              fontSize: 14,
              mb: 1,
            }}
          >
            Settings
          </ListSubheader>
        }
      >
        <ListItemButton>
          <ListItemIcon>
            <KeyIcon />
          </ListItemIcon>
          <ListItemText primary="API Keys" />
        </ListItemButton>
        <ListItemButton>
          <ListItemIcon>
            <TuneIcon />
          </ListItemIcon>
          <ListItemText primary="Preferences" />
        </ListItemButton>
      </List>
    </StyledDrawer>
  );
};
