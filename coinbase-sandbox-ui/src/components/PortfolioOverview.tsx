import React, { useEffect, useState } from "react";
import {
  Box,
  Card,
  CardContent,
  Typography,
  Grid,
  CircularProgress,
} from "@mui/material";

interface Account {
  uuid: string;
  name: string;
  currency: string;
  available_balance: {
    value: string;
    currency: string;
  };
  isDefault: boolean;
  active: boolean;
  created_at: string;
  updated_at: string;
  deleted_at: string | null;
  type: string;
}

export const PortfolioOverview: React.FC = () => {
  const [accounts, setAccounts] = useState<Account[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    fetch("/api/v3/brokerage/accounts")
      .then((res) => res.json())
      .then((data) => {
        setAccounts(data.accounts || []);
        setLoading(false);
      })
      .catch(() => setLoading(false));
  }, []);

  if (loading) {
    return <CircularProgress />;
  }

  return (
    <Box>
      <Typography variant="h6" sx={{ mb: 2 }}>
        Portfolio Overview
      </Typography>
      <Grid container spacing={2}>
        {accounts.map((account) => (
          <Grid xs={12} sm={6} md={3} key={account.uuid}>
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
