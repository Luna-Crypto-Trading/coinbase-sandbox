import React from "react";
import {
  Box,
  Card,
  CardContent,
  Typography,
  Grid,
  CircularProgress,
} from "@mui/material";
import { useAccounts } from "../hooks/useAccounts";

export const PortfolioOverview: React.FC = () => {
  const { data: accounts, isLoading } = useAccounts();

  if (isLoading) {
    return <CircularProgress />;
  }

  return (
    <Box>
      <Typography variant="h6" sx={{ mb: 2 }}>
        Portfolio Overview
      </Typography>
      <Grid container spacing={2}>
        {accounts?.map((account) => (
          <Grid size={{ xs: 12, sm: 6, md: 3 }} key={account.uuid}>
            <Card>
              <CardContent>
                <Typography variant="subtitle2" color="text.secondary">
                  {account.name} ({account.currency})
                </Typography>
                <Typography variant="h6">
                  {account.available_balance.value}
                </Typography>
                <Typography variant="caption" color="text.secondary">
                  Type: {account.type.replace("ACCOUNT_TYPE_", "")}
                </Typography>
              </CardContent>
            </Card>
          </Grid>
        ))}
      </Grid>
    </Box>
  );
};
