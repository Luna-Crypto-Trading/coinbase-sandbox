import { Box, Typography, Chip, Grid, Tabs, Tab, Paper } from "@mui/material";
import { useState } from "react";
import { PortfolioOverview } from "./PortfolioOverview";

interface TabPanelProps {
  children?: React.ReactNode;
  index: number;
  value: number;
}

function TabPanel(props: TabPanelProps) {
  const { children, value, index, ...other } = props;

  return (
    <div
      role="tabpanel"
      hidden={value !== index}
      id={`dashboard-tabpanel-${index}`}
      aria-labelledby={`dashboard-tab-${index}`}
      {...other}
    >
      {value === index && <Box sx={{ pt: 3 }}>{children}</Box>}
    </div>
  );
}

function a11yProps(index: number) {
  return {
    id: `dashboard-tab-${index}`,
    "aria-controls": `dashboard-tabpanel-${index}`,
  };
}

const Dashboard = () => {
  const [tabValue, setTabValue] = useState(0);

  const handleTabChange = (event: React.SyntheticEvent, newValue: number) => {
    setTabValue(newValue);
  };

  return (
    <Box
      sx={{
        minHeight: "100vh",
        bgcolor: "background.default",
        width: "100%",
        height: "100%",
        p: 3,
        boxSizing: "border-box",
      }}
    >
      {/* Header */}
      <Box
        sx={{
          display: "flex",
          justifyContent: "space-between",
          alignItems: "flex-start",
          mb: 4,
          flexWrap: "wrap",
          gap: 2,
        }}
      >
        <Box>
          <Typography variant="h1" component="h1" sx={{ mb: 1 }}>
            Coinbase Sandbox
          </Typography>
          <Typography variant="body1" color="text.secondary">
            Development & Testing Environment
          </Typography>
        </Box>
        <Chip label="SANDBOX MODE" className="sandbox-mode" />
      </Box>

      {/* Portfolio Overview */}
      <Box sx={{ mb: 4 }}>
        <PortfolioOverview />
      </Box>

      {/* Main Content Grid */}
      <Grid container spacing={3} sx={{ mb: 4 }}>
        {/* Trading Interface */}
        <Grid>
          <div>TradingInterface</div>
        </Grid>

        {/* Price Chart */}
        <Grid>
          <div>PriceChart</div>
        </Grid>
      </Grid>

      {/* Bottom Section with Tabs */}
      <Paper className="dashboard-paper" elevation={0}>
        <Box sx={{ borderBottom: "1px solid #e2e8f0" }}>
          <Tabs value={tabValue} onChange={handleTabChange} sx={{ px: 2 }}>
            <Tab label="Order History" {...a11yProps(0)} />
            <Tab label="Account Balances" {...a11yProps(1)} />
            <Tab label="Transaction History" {...a11yProps(2)} />
          </Tabs>
        </Box>

        <TabPanel value={tabValue} index={0}>
          <div>OrderHistoryTable</div>
        </TabPanel>

        <TabPanel value={tabValue} index={1}>
          <div>AccountBalancesTable</div>
        </TabPanel>

        <TabPanel value={tabValue} index={2}>
          <div>TransactionHistory</div>
        </TabPanel>
      </Paper>
    </Box>
  );
};

export { Dashboard };
