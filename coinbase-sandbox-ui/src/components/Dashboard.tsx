import { Box, Chip, Grid, Tabs, Tab } from "@mui/material";
import { useState } from "react";
import { PortfolioOverview } from "./PortfolioOverview";
import { PageLayout } from "./PageLayout";

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
    <PageLayout
      title="Coinbase Sandbox"
      subtitle="Development & Testing Environment"
      actions={<Chip label="SANDBOX MODE" className="sandbox-mode" />}
    >
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
    </PageLayout>
  );
};

export { Dashboard };
