import React, { useEffect, useState } from "react";
import {
  Box,
  Card,
  CardContent,
  Typography,
  Grid,
  CircularProgress,
  Button,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  Alert,
  Snackbar,
} from "@mui/material";
import { Add as AddIcon, Remove as RemoveIcon } from "@mui/icons-material";

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

interface TransactionDialogProps {
  open: boolean;
  onClose: () => void;
  account: Account | null;
  type: "deposit" | "withdraw";
  onSubmit: (amount: string) => void;
}

const TransactionDialog: React.FC<TransactionDialogProps> = ({
  open,
  onClose,
  account,
  type,
  onSubmit,
}) => {
  const [amount, setAmount] = useState("");
  const [error, setError] = useState("");

  const handleSubmit = () => {
    if (!amount || isNaN(Number(amount)) || Number(amount) <= 0) {
      setError("Please enter a valid amount");
      return;
    }
    onSubmit(amount);
    setAmount("");
    setError("");
    onClose();
  };

  return (
    <Dialog open={open} onClose={onClose}>
      <DialogTitle>
        {type === "deposit" ? "Deposit Funds" : "Withdraw Funds"}
      </DialogTitle>
      <DialogContent>
        <Box sx={{ pt: 2 }}>
          <TextField
            autoFocus
            label="Amount"
            type="number"
            fullWidth
            value={amount}
            onChange={(e) => setAmount(e.target.value)}
            error={!!error}
            helperText={error}
            InputProps={{
              endAdornment: account?.currency,
            }}
          />
        </Box>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose}>Cancel</Button>
        <Button onClick={handleSubmit} variant="contained">
          {type === "deposit" ? "Deposit" : "Withdraw"}
        </Button>
      </DialogActions>
    </Dialog>
  );
};

export const Portfolio: React.FC = () => {
  const [accounts, setAccounts] = useState<Account[]>([]);
  const [loading, setLoading] = useState(true);
  const [selectedAccount, setSelectedAccount] = useState<Account | null>(null);
  const [transactionType, setTransactionType] = useState<
    "deposit" | "withdraw"
  >("deposit");
  const [dialogOpen, setDialogOpen] = useState(false);
  const [snackbar, setSnackbar] = useState({
    open: false,
    message: "",
    severity: "success" as "success" | "error",
  });

  const fetchAccounts = async () => {
    try {
      const response = await fetch("/api/v3/brokerage/accounts");
      const data = await response.json();
      setAccounts(data.accounts || []);
    } catch (error) {
      console.error("Error fetching accounts:", error);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchAccounts();
  }, []);

  const handleTransaction = async (amount: string) => {
    if (!selectedAccount) return;

    try {
      const endpoint = transactionType === "deposit" ? "deposit" : "withdraw";
      const response = await fetch(
        `/api/v3/brokerage/sandbox/accounts/${selectedAccount.uuid}/${endpoint}`,
        {
          method: "POST",
          headers: {
            "Content-Type": "application/json",
          },
          body: JSON.stringify({
            amount: amount,
            currency: selectedAccount.currency,
          }),
        }
      );

      if (!response.ok) {
        throw new Error("Transaction failed");
      }

      setSnackbar({
        open: true,
        message: `${
          transactionType === "deposit" ? "Deposit" : "Withdrawal"
        } successful!`,
        severity: "success",
      });

      // Refresh account data
      fetchAccounts();
    } catch (error) {
      setSnackbar({
        open: true,
        message: `Transaction failed: ${
          error instanceof Error ? error.message : "Unknown error"
        }`,
        severity: "error",
      });
    }
  };

  const handleOpenDialog = (account: Account, type: "deposit" | "withdraw") => {
    setSelectedAccount(account);
    setTransactionType(type);
    setDialogOpen(true);
  };

  if (loading) {
    return <CircularProgress />;
  }

  return (
    <Box sx={{ width: "100%", marginLeft: 0 }}>
      <Typography variant="h4" sx={{ mb: 4 }}>
        Portfolio Management
      </Typography>
      <Grid container spacing={3} sx={{ width: "100%" }}>
        {accounts.map((account) => (
          <Grid
            sx={{
              width: {
                xs: "100%",
                sm: "calc(50% - 12px)",
                md: "calc(33.33% - 16px)",
              },
            }}
            key={account.uuid}
          >
            <Card>
              <CardContent>
                <Typography variant="h6" gutterBottom>
                  {account.name}
                </Typography>
                <Typography variant="h4" sx={{ mb: 2 }}>
                  {account.available_balance.value} {account.currency}
                </Typography>
                <Typography
                  variant="body2"
                  color="text.secondary"
                  sx={{ mb: 2 }}
                >
                  Type: {account.type.replace("ACCOUNT_TYPE_", "")}
                </Typography>
                <Box sx={{ display: "flex", gap: 1 }}>
                  <Button
                    variant="contained"
                    startIcon={<AddIcon />}
                    onClick={() => handleOpenDialog(account, "deposit")}
                    fullWidth
                  >
                    Deposit
                  </Button>
                  <Button
                    variant="outlined"
                    startIcon={<RemoveIcon />}
                    onClick={() => handleOpenDialog(account, "withdraw")}
                    fullWidth
                  >
                    Withdraw
                  </Button>
                </Box>
              </CardContent>
            </Card>
          </Grid>
        ))}
      </Grid>

      <TransactionDialog
        open={dialogOpen}
        onClose={() => setDialogOpen(false)}
        account={selectedAccount}
        type={transactionType}
        onSubmit={handleTransaction}
      />

      <Snackbar
        open={snackbar.open}
        autoHideDuration={6000}
        onClose={() => setSnackbar({ ...snackbar, open: false })}
      >
        <Alert
          onClose={() => setSnackbar({ ...snackbar, open: false })}
          severity={snackbar.severity}
          sx={{ width: "100%" }}
        >
          {snackbar.message}
        </Alert>
      </Snackbar>
    </Box>
  );
};
